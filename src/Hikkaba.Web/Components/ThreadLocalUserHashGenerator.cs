using Hikkaba.Service;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Components
{
    // todo: make it service and inject to views
    public class ThreadLocalUserHashGenerator : ViewComponent
    {
        private readonly ICryptoService _cryptoService;

        public ThreadLocalUserHashGenerator(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public string Invoke(string threadId, string userIpAddress)
        {
            return _cryptoService.HashHmacHex(threadId, userIpAddress);
        }
    }
}