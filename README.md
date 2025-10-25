# Senior Event Booking - Umbraco 13 CMS

A professional event booking system built with Umbraco 13 CMS, featuring member-only bookings and seamless integration with Memberbase CRM API.

## 🚀 Project Overview

This project demonstrates a complete event management and booking system for a Not-for-Profit organisation. It showcases:

- **Event Management**: Create and display upcoming events with comprehensive details
- **Member-Only Bookings**: Secure booking system restricted to authenticated members
- **CRM Integration**: Automatic contact synchronization with Memberbase CRM
- **Professional UI/UX**: Modern, responsive design with excellent user experience
- **Data Persistence**: Robust database architecture for booking management

## 📋 Features Implemented

### ✅ Core Functionality
- [x] Event listing page with card-based layout
- [x] Individual event detail pages with full information
- [x] Member authentication system
- [x] Booking form with comprehensive validation
- [x] Real-time capacity tracking
- [x] Database persistence for all bookings
- [x] Memberbase CRM API integration
- [x] Success/error feedback messaging
- [x] Responsive design for all devices

### 🎨 User Interface
- Modern, clean design with professional aesthetics
- Color-coded badges for event status (Available, Selling Fast, Sold Out)
- SVG icon library for visual clarity
- Smooth animations and transitions
- Mobile-first responsive layout

### 🔒 Security Features
- Member authentication required for bookings
- Anti-forgery token validation
- Server-side input validation
- SQL injection protection via Entity Framework
- Secure API communication

## 🛠️ Technical Architecture

### Backend Components

#### **Models**
- `EventBooking.cs` - Entity model for booking data
- `BookingFormViewModel.cs` - Form validation model

#### **Data Layer**
- `EventBookingDbContext.cs` - EF Core database context
- Umbraco migration system for schema management
- SQL Server database with indexed columns

#### **Services**
- `IMemberbaseService.cs` - CRM integration interface
- `MemberbaseService.cs` - HTTP client-based API service with error handling

#### **Controllers**
- `BookingController.cs` - Surface controller handling form submissions
- Comprehensive validation and error handling
- TempData for user feedback

### Frontend Components

#### **Views**
- `EventList.cshtml` - Events listing page
- `Event.cshtml` - Event detail page
- `_BookingForm.cshtml` - Booking form partial

#### **Styling**
- Custom CSS with CSS variables for theming
- Responsive grid layouts
- Component-based styling approach

### Database Schema

**EventBookings Table:**
```sql
- Id (INT, Primary Key, Identity)
- EventKey (GUID, Indexed) - Links to Umbraco content node
- BookerName (NVARCHAR(200), Required)
- BookerEmail (NVARCHAR(255), Required, Indexed)
- Note (NVARCHAR(1000), Optional)
- CreatedDate (DATETIME, Required, Indexed, Default: GETUTCDATE())
- ApiContactId (NVARCHAR(100), Optional)
- ApiResponse (NVARCHAR(500), Optional)
- ApiSuccess (BIT, Required, Default: 0)
```

## 🔗 API Integration

### Memberbase CRM
- **Endpoint**: `https://demo-log.memberbase-sandbox.com/api/contacts`
- **Authentication**: Bearer token (JWT)
- **Method**: POST
- **Payload**: `{ name, email, source }`

#### Integration Flow:
1. User submits booking form
2. Validation performed server-side
3. Booking saved to local database
4. Async API call to Memberbase
5. API response stored with booking record
6. User receives confirmation message

## 📦 Dependencies

```xml
<PackageReference Include="Umbraco.Cms" Version="13.9.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
<PackageReference Include="Microsoft.CodeAnalysis.Common" Version="4.10.0" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.10.0" />
```

## ⚙️ Configuration

### Database Connection
Located in `appsettings.Development.json`:
```json
"ConnectionStrings": {
  "umbracoDbDSN": "Data Source=.\\SQLEXPRESS;Database=Umbraco;...",
  "EventBookingDb": "Data Source=.\\SQLEXPRESS;Database=Umbraco;..."
}
```

### Memberbase API
```json
"Memberbase": {
  "BaseUrl": "https://demo-log.memberbase-sandbox.com",
  "ApiKey": "[JWT Token]"
}
```

## 🚦 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server Express (LocalDB)
- Visual Studio 2022 or VS Code
- Git

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/Operakid619/HTYF-UMBRACO-PROJECT.git
   cd HTYF-UMBRACO-PROJECT
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the site**
   - Front-end: `https://localhost:44372` or `http://localhost:16349`
   - Back office: `https://localhost:44372/umbraco`

### Default Credentials
- **Username**: admin
- **Email**: developer@senior.co.uk
- **Password**: Assessment123!

## 📝 Creating Document Types

The following Document Types need to be created in the Umbraco back office:

### EventList Document Type
- **Alias**: `eventList`
- **Template**: EventList
- **Properties**: None required (uses child events)
- **Allowed Child Types**: Event

### Event Document Type
- **Alias**: `event`
- **Template**: Event
- **Properties**:
  - **Title** (inherited from Name)
  - **Summary**: Rich Text Editor
  - **EventDateTime**: Date/Time Picker
  - **Location**: Text String
  - **Capacity**: Integer
- **Allowed at Root**: No
- **Allowed Child Types**: None

## 🎯 Operational Excellence (OpEx) Compliance

This project demonstrates adherence to operational excellence principles:

### 1. **Standardization**
- Consistent naming conventions across all code
- Structured folder organization (Models, Services, Controllers, Data)
- Standard Umbraco architectural patterns followed
- Uniform code formatting and style

### 2. **Documentation**
- Comprehensive XML comments on all classes and methods
- Clear README with setup instructions
- Inline code comments for complex logic
- Commit messages following conventional commits format

### 3. **Error Handling & Logging**
```csharp
// Example from MemberbaseService.cs
try {
    _logger.LogInformation("Creating contact...");
    // API call logic
} catch (HttpRequestException ex) {
    _logger.LogError(ex, "Network error");
    return (false, null, "Network error message");
}
```

### 4. **Scalability & Maintainability**
- Dependency injection for loose coupling
- Interface-based service layer
- Separation of concerns (MVC pattern)
- Entity Framework for database abstraction

### 5. **Security Best Practices**
- Parameterized queries via EF Core
- Anti-forgery tokens on forms
- Member authentication checks
- Input validation at multiple levels

## 🧪 Testing Approach

### Manual Testing Checklist
- [ ] Event list displays correctly
- [ ] Event detail pages show all information
- [ ] Non-members see login prompt
- [ ] Members can access booking form
- [ ] Form validation works correctly
- [ ] Bookings save to database
- [ ] API integration creates contacts
- [ ] Success/error messages display properly
- [ ] Responsive design on mobile devices

## 📊 Project Structure

```
SeniorEventBooking/
├── Controllers/
│   └── BookingController.cs
├── Data/
│   └── EventBookingDbContext.cs
├── Migrations/
│   ├── CreateEventBookingsTable.cs
│   └── EventBookingMigrationComposer.cs
├── Models/
│   ├── EventBooking.cs
│   └── BookingFormViewModel.cs
├── Services/
│   ├── IMemberbaseService.cs
│   └── MemberbaseService.cs
├── Views/
│   ├── Event.cshtml
│   ├── EventList.cshtml
│   └── Partials/
│       └── _BookingForm.cshtml
├── wwwroot/
│   └── css/
│       └── site.css
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

## 🔄 Development Workflow

All development follows a structured Git workflow with professional commits:

```bash
# Feature development
git add .
git commit -m "feat: add new feature description"
git push origin main

# Bug fixes
git commit -m "fix: resolve specific issue"

# Documentation
git commit -m "docs: update README with new information"
```

## 🎓 Learning Outcomes

This project demonstrates proficiency in:
- Umbraco 13 CMS development
- .NET 8 web development
- Entity Framework Core
- RESTful API integration
- Responsive web design
- Database schema design
- Dependency injection
- Async/await patterns
- Error handling and logging
- Version control with Git

## 📞 Support

For questions or issues, please contact:
- **Developer**: developer@senior.co.uk
- **GitHub**: [@Operakid619](https://github.com/Operakid619)

## 📄 License

This project is created as part of a technical assessment for Senior Internet.

---

**Built with** ❤️ **using Umbraco 13 and .NET 8**
