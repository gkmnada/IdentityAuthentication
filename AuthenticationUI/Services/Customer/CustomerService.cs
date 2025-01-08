using AuthenticationUI.Models;

namespace AuthenticationUI.Services.Customer
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;

        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CustomerViewModel>> ListCustomerAsync()
        {
            var response = await _httpClient.GetAsync("customer");
            return await response.Content.ReadFromJsonAsync<List<CustomerViewModel>>();
        }
    }
}
