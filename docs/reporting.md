# Reporting with YASEM

In addition to YASEM inner logging and console reporting, you can add a cusltom reporter like [ExtentReports](https://www.extentreports.com/docs/versions/5/net/index.html) in the "Execute" method of the "ValidationEngine" class as follows: 

```C#
using XXXX; //TODO: Add sample code for adding Extent Reports

```

For the main features, YASEM was intentionally built without the support of a UnitTesting Framework (like NUNIT or MSTEst) to reduce the complexity level of the tests by requiring no programming skills other than how to structure a simple JSON file. But you can implement your tets using the Test Unit Framework and take advantage of its features like test case pre/post conditions and reporting by modifiying the ValidationEngine and Validator class accordingly. 

You could also completely revamp YASEM's simple structure to ditch the YASEM main class and convert it into a Unit Testing project and even implement BDD technologies like [SpecFlow](https://specflow.org/).

By leveraging the advantages of Unit Testign frameworks would also gain the possibility to implement of other advanced reporting tools such as [Allure](https://docs.qameta.io/allure/#__net) but at the cost of requiring more advanced programming killed personnel within your team.

Note that by adding a test reporting tool, you may need to map the Result.Status property in the ValidationResult class to the ones available in the test reporting tool of your choosing (e.g., for [ExtentReports](https://github.com/extent-framework/extentreports-csharp/blob/master/ExtentReports/Core/Status.cs) extra "Info" and "Error" status).