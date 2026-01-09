Create an API Client library project using .NET 10 that meets the following requirements:
1. **Project Setup**:
   - Use .NET 10 as the target framework.
2. **API Client Functionality**:
	1. Implement a class named `ApiClient` that can perform HTTP requests.
	   - Use `HttpClient` for making HTTP requests. (You may use RestSharp if you wish to.)
	   - Include methods `GetAsync`, `PostAsync`, `DeleteAsync` and `GetfromJSON` for the required endpoints mentioned in 12.
	2. **Error Handling**:
	   - Implement robust error handling for network issues and HTTP errors.
	3. **Configuration**:
	   - Allow configuration of the `base URL` for the API and `Token`.
	   - Support setting custom headers for requests e.g. authentication tokens.
	4. **Asynchronous Programming**:
	   - Ensure that all network operations are performed asynchronously using `async` and `await`.
	5. **Logging**:
	   - Implement basic logging to track request and response details.
	6. Authentication:
	   - Support a header `X-API-Key` with the value set to the Token. This is required to access the delete endpoints.
	7. **Serialization**:
	   - Use `System.Text.Json` for JSON serialization and deserialization. (Where applicable).
	8. **Documentation**:
		- None Required (It is suggested that you use basic comments in the code to explain the functionality of methods and classes.)
	9. **Testing**:
		- None Required (It is suggested that you write unit tests to verify the functionality of the API client methods.)
	10. **Dependencies**:
		- Use only publicly available NuGet packages.
	11. **Code Quality**:
		- Follow best practices for code readability and maintainability.
	12. **Example Usage**:
		- Provide a simple example demonstrating how to use the `ApiClient` to make GET, POST and DELETE requests.
			1. Demonstrate handling responses and errors.
			2. Show how to configure the client with a base URL and custom headers.
				- Required Example Usage:
					- GetAll Customer Details looping through each customer, invoice and phone numbers.
					- Using Post create a New Customer. This MUST include at least 3 phone numbers and invoices. Which must be provided in a customer object.
					- Try create the same customer again to show error handling for duplicate entries.
					- Create an example of Delete Customer to remove a customer ID 10. Make sure to use the API key
					- Call this twice to show error handling when trying to delete a non-existing customer ID 10 again.
					- Add an invoice to an existing customer ID 5.
					- Add an example of error handling when trying to add an invoice to a non-existing customer ID 99.