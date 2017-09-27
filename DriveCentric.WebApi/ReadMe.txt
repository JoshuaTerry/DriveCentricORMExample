
To run the example.

1. Open the Web.Config found in DriveCentric.WebApi
2. Edit the "DomainContext" Connection string and set your Data Source and Database name where indicated
3. Open Package Manager Console
4. Change the Default Project Drop Down in the Package Manager Console Window to DriveCentric.Data
5. Type and execute the following command:  update-database

This will create the database as well as 2 seed records in the Customers Table.

6. Set DriveCentric.WebApi as the default project for the solution and Run
7.  In postman exectute a git on the your local instance for /api/v1/customers

Output:
{
    "Data": [
        {
            "Id": "cd614a45-4cac-4ae3-b446-e29d0b4002ab",
            "FirstName": "Joshua",
            "LastName": "Terry",
            "Email": "jterry@drivecentric.com",
            "DateOfBirth": "0001-01-01T00:00:00"
        },
        {
            "Id": "f477cfe8-6d58-47c7-a7eb-39b13e1746e1",
            "FirstName": "Bob",
            "LastName": "Alouie",
            "Email": "bob@drivecentric.com",
            "DateOfBirth": "0001-01-01T00:00:00"
        }
    ],
    "TotalResults": 2,
    "IsSuccessful": true,
    "ErrorMessages": [],
    "VerboseErrorMessages": []
}


