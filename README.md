# Currency Converter Web API (.NET 9)

## Overview  
This is a **.NET 9 Core Web API** project designed for building scalable RESTful services. It includes the best practices caching, DI and other coding standards. 
There are 3 main functions of api: [Get latest exchange rates with Base currecy EUR, Convert given amount from one currency to another, and get the paged historical exchange rates based on from date, to date and base currecy.
frankfurter open api is used as primary api, if primary api is not available then fixer.io is used as backup api.
Few improvements are needed to include **rate limiting** and add and make **unit testing** work.

---

## Features
✅ Built with **.NET 9 Core Web API**  
✅ **Rate Limiting** using ASP.NET Middleware (Missing for now) 
✅ **Caching** with `IMemoryCache`  
✅ **Dependency Injection (DI)**  
✅ **Unit Testing** with `xUnit` & `Moq` (TODO: Need to make it work) 
✅ **Swagger / OpenAPI** for API Documentation
✅ **Serilog is used for error logging and http request logging**  

---

## Getting Started

### Prerequisites
- **.NET 9 SDK** – [Download Here](https://dotnet.microsoft.com/download)  
- **Visual Studio Code** or **Visual Studio 2022**  
- **Postman** (for testing API requests)  

---

### Installation & Setup 

- **Clone the repository**
- **Create a free account on fixer.io, get the access key and add to appsettings** (as FixerKey)
- Clean, build, run and test the solution
- swager url should be: https://localhost:7289/index.html
- Tests will will be added and work soon....


---

### **Key Highlights**
✅ **Clear Project Overview**  
✅ **Setup & Installation Guide**  
✅ **API Documentation**  
✅ **Testing Instructions**  



