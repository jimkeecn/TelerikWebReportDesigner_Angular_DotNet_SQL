# Telerik Web Report Designer Integration Demo

## Project Description

This project demonstrates a potential approach to integrating the **Telerik Web Report Designer** into an application built with **Angular, .NET, and SQL Server**. It aims to showcase how Telerik Reporting can be embedded and customized in a secure and streamlined way without building a reporting system from scratch.

The project is intended as a proof of concept, illustrating how this integration can be extended into real-world systems using technologies like **Dapper** and **Elasticsearch**, although this demo uses **Entity Framework** for simplicity.

---

## Features (Out-of-the-Box)

1. **Dynamic WebServiceDataSource Injection**

   - Allows the backend to inject API endpoints into report templates dynamically.
   - Enables users to focus on designing report layouts while leaving data source configuration to the server logic.
   - Eliminates the need to define data sources manually in the template.

2. **Secure Token-Based Authentication**
   - Implements a mock OAuth2 authentication system to demonstrate how tokens can be injected on-the-fly during report preview and data fetch.
   - Avoids embedding sensitive authentication information in report templates.
   - Ensures that API calls made through the WebServiceDataSource are properly secured without exposing credentials.

---

## Technologies Used

- **Frontend:** Angular v16
- **Backend:** ASP.NET Core v8.0
- **Database:** SQL Server
- **Authentication (Demo):** Mock OAuth2 with bearer token handling
- **Reporting:** Telerik Web Report Designer 17.1.23.718 + Telerik Reporting Engine 17.1.23.718

---

## How It Works

- The Telerik Web Report Designer is embedded as a native HTML component (not an iframe), allowing full control over the layout and DOM elements.
- Reports are saved to and retrieved from a SQL database, including metadata and layout definitions.
- The WebServiceDataSource used in each report is dynamically generated and protected by token-based authentication.
- Authentication tokens are injected automatically during report rendering or preview, preventing sensitive data from being stored in templates.

---

## Usage

1. Run the backend project to enable report APIs, authentication logic, and template storage.
2. Start the Angular frontend to load the embedded Telerik Web Report Designer.
3. Log in with the mock authentication flow.
4. Create or edit a report using the designer, and preview the result with live, authenticated data pulled through the backend APIs.

---

## Notes

- This is a proof-of-concept and not intended for production use as-is.
- This project is directly override on an existing official recommanded project by Telerik Team.

---

## License

This project is intended for internal evaluation and demonstration purposes only.
