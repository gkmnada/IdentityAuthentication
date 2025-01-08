using AuthenticationUI.Models;

namespace AuthenticationUI.Services.Customer
{
    public interface ICustomerService
    {
        Task<List<CustomerViewModel>> ListCustomerAsync();
    }
}
