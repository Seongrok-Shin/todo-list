using Supabase;
using System.Text.RegularExpressions;
using Supabase.Postgrest;
using ToDoWebApp.Models;

namespace ToDoWebApp.Services
{
    public class TodoService
    {
        private readonly Supabase.Client _supabaseClient;

        public TodoService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<TodoItem>> GetTodoItemsAsync()
        {
            var response = await _supabaseClient.From<TodoItem>()
                .Order(x => x.Status, Constants.Ordering.Ascending)
                .Order(x => x.CreatedAt, Constants.Ordering.Descending)
                .Get();
            return response.Models;
        }

        public async Task AddTodoItemsFromTextAsync(string todoText)
        {
            if (string.IsNullOrWhiteSpace(todoText))
            {
                throw new ArgumentException("Enter ToDo item");
            }

            var lines = todoText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var newTodoItems = new List<TodoItem>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                var todoItem = new TodoItem();
                var regex = new Regex(@"^\[(Done|WIP)\]\s*\[([A-Za-z0-9\/]+)\]\s*(.*)$");
                var match = regex.Match(trimmedLine);

                if (match.Success)
                {
                    todoItem.Status = match.Groups[1].Value;
                    todoItem.Category = match.Groups[2].Value;
                    todoItem.Description = match.Groups[3].Value.Trim();
                }
                else
                {
                    todoItem.Status = "WIP";
                    todoItem.Category = "General";
                    todoItem.Description = trimmedLine;
                }

                newTodoItems.Add(todoItem);
            }

            if (newTodoItems.Any())
            {
                await _supabaseClient.From<TodoItem>().Insert(newTodoItems);
            }
        }

        public async Task UpdateTodoItemStatusAsync(Guid id, string newStatus)
        {
            var response = await _supabaseClient.From<TodoItem>()
                .Where(x => x.Id == id)
                .Set(x => x.Status, newStatus)
                .Update();
            if (!response.Models.Any())
            {
                throw new InvalidOperationException($"Cannot find ID {id} in the TODO List.");
            }
        }

        public async Task DeleteTodoItemAsync(Guid id)
        {
            // Check ID is existed
            var existingItem = await _supabaseClient.From<TodoItem>()
                .Where(x => x.Id == id)
                .Get();

            if (!existingItem.Models.Any())
            {
                throw new InvalidOperationException($"Cannot find ID {id} in the TODO List.");
            }

            // If the item exists, delete it.
            await _supabaseClient.From<TodoItem>()
                .Where(x => x.Id == id)
                .Delete();
        }
    }
}