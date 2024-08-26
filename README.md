**Introduction**
This assessment contains two projects one is KBMGrpcService and another one is KBMHttpService. These projects are developed on .Net Core 6.0 framework. 
KBMGrpcService: This is a GRPC project which has the services for Organization and Users. There are all CRUD operations included in the GRPC project. It includes Unit tests and integration tests.
KBMHttpService: This is a WebAPI project, which is consuming KBMGrpcService endpoints. This project includes all CRUD oprations of Organization and Users including association and disssociation of users with the organizations. 

**Project Setup**

1.Clone the repositories using following links:
KBMGrpcService: https://github.com/raj-mobile-programming/KBMGrpcServices.git
KBMHttpService: https://github.com/raj-mobile-programming/KBMGrpcServices.git

2. Install Docker desktop
Download image from following command
docker pull mcr.microsoft.com/mssql/server:2022-latest

3. Setup download image on the docker and run the docker.

4. Start MSSQL Management Studio, check connection with database. 

5. Before first run of the KBMGrpcService project, run the migrations using following commands

--dotnet ef migrations add InitialCreate
--dotnet ef database update

6. Run KBMGrpcService project, on the command window check the port for https.

7.Update the port number for https on which KBMGrpcService is running in KBMHttpService.Program.cs file.

8. Now run KBMHttpService project.

9. Now you are ready to use the endpoints for Organization and for Users

10. To run Unit Tests and Integration Tests
In visual studio Click>Tests > Test Explorer > Run All Tests

**Project Structure**

├── KBMGrpcService
│   ├── Protos
│   │   └── (Proto files for gRPC services)
│   ├── Services
│   │   ├── OrganizationService.cs
│   │   └── UserService.cs
│   ├── Data
│   │   ├── AppDbContext.cs
│   │   └── Models
│   │       ├── OrganizationModel.cs
│   │       └── UserModel.cs
│   ├── Tests
│   │   ├── UnitTests
│   │   │   ├── OrganizationServiceTests.cs
│   │   │   └── UserServiceTests.cs
│   │   └── IntegrationTests
│   │       ├── OrganizationServiceIntegrationTests.cs
│   │       └── UserServiceIntegrationTests.cs
│   └── Program.cs
├── KBMHttpService
│   ├── Controllers
│   │   ├── OrganizationsController.cs
│   │   └── UsersController.cs
│   ├── Models
│   │   ├── OrganizationRequestModel.cs
│   │   ├── UserRequestModel.cs
│   │   └── (Response models)
│   ├── Tests
│   │   ├── UnitTests
│   │   │   ├── OrganizationsControllerTests.cs
│   │   │   └── UsersControllerTests.cs
│   │   └── IntegrationTests
│   │       ├── OrganizationsControllerIntegrationTests.cs
│   │       └── UsersControllerIntegrationTests.cs
│   └── Program.cs
└── README.md
