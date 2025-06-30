# TODO List Web Application

A modern, full-stack TODO list application built with .NET 9 and Blazor Server, featuring user authentication and real-time data management with Supabase.

## 🚀 Features

- **User Authentication**: Secure sign-up and login functionality
- **Personal TODO Management**: Create, read, update, and delete personal TODO items
- **Category Organization**: Organize tasks by categories
- **Status Tracking**: Track task completion status (TODO, IN PROGRESS, DONE)
- **Responsive Design**: Modern dark theme with responsive layout
- **Real-time Updates**: Live data synchronization with Supabase
- **Bulk Operations**: Select and delete multiple tasks at once

## 🛠️ Tech Stack

### Backend
- **Framework**: .NET 9.0
- **Platform**: Blazor Server (Server-Side Rendering)
- **Language**: C#
- **Authentication**: Supabase Auth
- **Database**: PostgreSQL (via Supabase)

### Frontend
- **UI Framework**: Blazor Components
- **Styling**: Bootstrap 5 + Custom CSS
- **Icons**: Bootstrap Icons
- **Fonts**: VT323 (Monospace), Inter, Open Sans
- **Theme**: Dark theme with custom styling

### Database & Backend Services
- **Database**: Supabase (PostgreSQL)
- **Real-time**: Supabase Realtime
- **Authentication**: Supabase GoTrue
- **Storage**: Supabase Storage

### Key Dependencies
- **Supabase**: `1.1.1` - Complete Supabase client
- **DotNetEnv**: `3.1.1` - Environment variable management
- **Microsoft.AspNetCore.Components.DataAnnotations.Validation**: Data validation

## 📁 Project Structure

```
ToDoWebApp/
├── Components/
│   ├── Layout/           # Layout components (MainLayout, NavMenu, ThemeToggle)
│   └── Pages/            # Page components (Home, Login, SignUp, ToDoList, etc.)
├── Models/
│   ├── LoginModel.cs     # Login form model
│   └── ToDoItems.cs      # TODO item data model
├── Services/
│   ├── AuthService.cs    # Authentication service
│   └── TodoService.cs    # TODO CRUD operations service
├── wwwroot/             # Static files (CSS, images)
├── Program.cs           # Application entry point
└── ToDoWebApp.csproj    # Project configuration
```

## 🚦 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Supabase account and project
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/todo-list.git
   cd todo-list/ToDoWebApp
   ```

2. **Set up environment variables**
   Create a `.env` file in the ToDoWebApp directory:
   ```env
   SUPABASE_URL=your_supabase_project_url
   SUPABASE_ANON_KEY=your_supabase_anon_key
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Set up Supabase Database**
   Create a `todos` table in your Supabase project with the following schema:
   ```sql
   CREATE TABLE todos (
       id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
       user_id UUID NOT NULL,
       status TEXT NOT NULL DEFAULT 'TODO',
       category TEXT NOT NULL,
       description TEXT NOT NULL,
       created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
       updated_at TIMESTAMP WITH TIME ZONE
   );
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Open your browser**
   Navigate to `https://localhost:5001` or `http://localhost:5000`

## 🔧 Configuration

The application uses environment variables for configuration:

- `SUPABASE_URL`: Your Supabase project URL
- `SUPABASE_ANON_KEY`: Your Supabase anonymous key

You can set these in:
- `.env` file (recommended for development)
- System environment variables
- `appsettings.json` configuration

## 📱 Usage

1. **Sign Up/Login**: Create an account or login with existing credentials
2. **View Tasks**: Navigate to the TODO list to see your personal tasks
3. **Add Tasks**: Click "Add New TODO" to create tasks with categories
4. **Manage Tasks**: 
   - Edit task details
   - Change status (TODO → IN PROGRESS → DONE)
   - Delete individual tasks
   - Bulk delete multiple tasks
5. **Categories**: Organize tasks by custom categories

## 🎨 Features in Detail

### Authentication System
- Secure user registration and login
- Session management with Supabase Auth
- Protected routes requiring authentication

### Task Management
- **CRUD Operations**: Full create, read, update, delete functionality
- **Status Workflow**: TODO → IN PROGRESS → DONE
- **Categories**: Custom categorization system
- **Timestamps**: Automatic creation and update tracking

### User Interface
- **Dark Theme**: Modern dark interface with VT323 monospace font
- **Responsive Design**: Works on desktop and mobile devices
- **Interactive Components**: Real-time status updates and bulk operations
- **Accessibility**: Proper ARIA labels and semantic HTML

## 🐳 Docker Support

The application includes a Dockerfile for containerization:

```bash
# Build the Docker image
docker build -t todoapp .

# Run the container
docker run -p 8080:8080 --env-file .env todoapp
```

## 🤝 Contributing

This project was developed with the assistance of **GitHub Copilot** for enhanced coding productivity and best practices implementation.

### Development Guidelines
- Follow C# coding conventions
- Use Blazor component best practices
- Maintain responsive design principles
- Write meaningful commit messages
- Test authentication flows thoroughly

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 🙏 Acknowledgments

- **GitHub Copilot**: AI-powered coding assistance for faster development
- **Supabase**: Backend-as-a-Service platform
- **Bootstrap**: CSS framework for responsive design
- **.NET Community**: For excellent documentation and support

## 📞 Support

If you encounter any issues or have questions:
1. Check the existing issues in the repository
2. Create a new issue with detailed description
3. Ensure your Supabase configuration is correct
4. Verify all environment variables are set properly

---

**Built with ❤️ using .NET 9, Blazor Server, and GitHub Copilot**
