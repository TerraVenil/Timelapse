# Timelapse AWS Lambda Function .NET Core Project

## Here are some steps to follow to get started from the command line:

Once you have edited your function you can use the following command lines to build, test and deploy your function to AWS Lambda from the command line:

Restore dependencies
```
    cd "Timelapse"
    npm i
    dotnet restore
```

Execute unit tests
```
    cd "Timelapse.Tests"
    dotnet test
```

Deploy function to AWS Lambda
```
    cd "Timelapse"
    dotnet lambda deploy-function
```