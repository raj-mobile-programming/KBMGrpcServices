**Introduction**

This assessment contains two projects one is KBMGrpcService and another one is KBMHttpService. These projects are developed on .Net Core 6.0 framework. 

**KBMGrpcService:** This is a GRPC project which has the services for Organization and Users. There are all CRUD operations included in the GRPC project. It includes Unit tests and integration tests.

**KBMHttpService:** This is a WebAPI project, which is consuming KBMGrpcService endpoints. This project includes all CRUD oprations of Organization and Users including association and disssociation of users with the organizations. 

**Project Setup**

1. Clone HTTP Services project repository using following link
KBMHttpService: https://github.com/raj-mobile-programming/KBMGrpcServices.git

2. Install Docker desktop
Download image from following command
docker pull mcr.microsoft.com/mssql/server:2022-latest

3. Setup download image on the docker and run the docker.

4. Start MSSQL Management Studio, check connection with database. 

5. Before first run of the KBMGrpcService project, run the migrations using following commands

**For package manager**

```update-database```

**For command prompt**

```dotnet ef database update```

6. Run KBMGrpcService project, on the command window check the port for https.

7. Now you KBMGrpcService project setup is ready to use for the endpoints of Organization and for Users.

10. To run Unit Tests and Integration Tests
In visual studio Click>Tests > Test Explorer > Run All Tests

**Endpoints**

KBMGrpcService

Create Organization

Get Organization by ID

Query Organizations (paginated)

Update Organization

Delete Organization

Create User

Get User by ID

Query Users (paginated)

Update User

Delete User

Associate User to Organization

Disassociate User from Organization







