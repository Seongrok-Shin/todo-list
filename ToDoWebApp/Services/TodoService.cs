using Supabase;
using System.Text.RegularExpressions;
using static Supabase.Postgrest.Constants;
using ToDoWebApp.Models;
using Microsoft.Extensions.Logging;

namespace ToDoWebApp.Services
{
    public class TodoService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly AuthService _authService;
        private readonly ILogger<TodoService> _logger;

        public TodoService(Supabase.Client supabaseClient, AuthService authService, ILogger<TodoService> logger)
        {
            _supabaseClient = supabaseClient;
            _authService = authService;
            _logger = logger;
        }

        // Parse and add TODO items from text
        public async Task AddTodoItemsFromTextAsync(string todoText)
        {
            if (string.IsNullOrWhiteSpace(todoText))
            {
                throw new ArgumentException("TODO text cannot be empty.");
            }

            if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to create TODO items.");
            }

            var lines = todoText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var todoItems = new List<ToDoItems>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                var todoItem = ParseTodoLine(trimmedLine);
                if (todoItem != null)
                {
                    todoItem.UserId = Guid.Parse(_authService.CurrentUser.Id);
                    todoItem.CreatedAt = DateTime.UtcNow;
                    todoItems.Add(todoItem);
                }
            }

            if (todoItems.Count == 0)
            {
                throw new ArgumentException("No valid TODO items found in the provided text.");
            }

            // Insert all TODO items
            try
            {
                await _supabaseClient.From<ToDoItems>().Insert(todoItems);
                _logger.LogInformation($"Successfully created {todoItems.Count} TODO items for user {_authService.CurrentUser.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting TODO items into database");
                throw;
            }
        }

        // Parse a single TODO line
        private ToDoItems? ParseTodoLine(string line)
        {
            // Regex pattern to match [STATUS] [CATEGORY] Description
            var pattern = @"^\[(\w+)\]\s*\[(\w+)\]\s*(.+)$";
            var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                _logger.LogWarning($"Invalid TODO format: {line}");
                return null;
            }

            var status = match.Groups[1].Value.ToUpper();
            var category = match.Groups[2].Value.ToUpper();
            var description = match.Groups[3].Value.Trim();

            // Validate status
            if (!IsValidStatus(status))
            {
                _logger.LogWarning($"Invalid status '{status}' in line: {line}");
                return null;
            }

            return new ToDoItems
            {
                Status = status,
                Category = category,
                Description = description
            };
        }

        // Validate if status is one of the allowed values
        private bool IsValidStatus(string status)
        {
            var validStatuses = new[] { "TODO", "WIP", "REVIEW", "DONE" };
            return validStatuses.Contains(status);
        }

        // Get all TODO items for current user
        public async Task<List<ToDoItems>> GetTodoItemsAsync()
        {
            if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to view TODO items.");
            }

            try
            {
                var userId = Guid.Parse(_authService.CurrentUser.Id);
                var response = await _supabaseClient
                    .From<ToDoItems>()
                    .Where(x => x.UserId == userId)
                    .Order(x => x.CreatedAt, Ordering.Descending)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TODO items from database");
                throw;
            }
        }

        // Update TODO item status
        public async Task UpdateTodoStatusAsync(Guid todoId, string newStatus)
        {
            if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to update TODO items.");
            }

            if (!IsValidStatus(newStatus))
            {
                throw new ArgumentException($"Invalid status: {newStatus}");
            }

            try
            {
                var userId = Guid.Parse(_authService.CurrentUser.Id);
                _logger.LogInformation($"Attempting to update TODO {todoId} to status {newStatus} for user {userId}");
                
                // 먼저 기존 항목을 가져옴
                var existingItems = await _supabaseClient
                    .From<ToDoItems>()
                    .Where(x => x.Id == todoId && x.UserId == userId)
                    .Get();

                if (!existingItems.Models.Any())
                {
                    throw new InvalidOperationException("TODO item not found or access denied.");
                }

                var existingItem = existingItems.Models.First();
                
                // 상태와 업데이트 시간만 변경
                existingItem.Status = newStatus;
                existingItem.UpdatedAt = DateTime.UtcNow;

                var result = await _supabaseClient
                    .From<ToDoItems>()
                    .Where(x => x.Id == todoId && x.UserId == userId)
                    .Update(existingItem);

                _logger.LogInformation($"Successfully updated TODO item {todoId} status to {newStatus}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating TODO item {todoId} to status {newStatus}: {ex.Message}");
                throw;
            }
        }

        // Delete TODO item
        public async Task DeleteTodoAsync(Guid todoId)
        {
            if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to delete TODO items.");
            }

            try
            {
                var userId = Guid.Parse(_authService.CurrentUser.Id);
                await _supabaseClient
                    .From<ToDoItems>()
                    .Where(x => x.Id == todoId && x.UserId == userId)
                    .Delete();

                _logger.LogInformation($"Deleted TODO item {todoId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting TODO item {todoId}");
                throw;
            }
        }

        // Delete all TODO items for current user
        public async Task DeleteAllTodosAsync()
        {
            if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to delete TODO items.");
            }

            try
            {
                var userId = Guid.Parse(_authService.CurrentUser.Id);
                
                // Check Items
                var itemsToDelete = await _supabaseClient
                    .From<ToDoItems>()
                    .Where(x => x.UserId == userId)
                    .Get();

                if (itemsToDelete.Models.Count == 0)
                {
                    _logger.LogInformation("No TODO items found to delete");
                    return;
                }

                // Delete All
                await _supabaseClient
                    .From<ToDoItems>()
                    .Where(x => x.UserId == userId)
                    .Delete();

                _logger.LogInformation($"Deleted all {itemsToDelete.Models.Count} TODO items for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all TODO items");
                throw;
            }
        }
    }
}