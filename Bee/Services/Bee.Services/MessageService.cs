using Bee.Services.Interfaces;
using MaterialDesignThemes.Wpf;

namespace Bee.Services
{
    public class MessageService : IMessageService
    {
        private readonly ISnackbarMessageQueue _messageQueue;

        public MessageService(ISnackbarMessageQueue  messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public void Notice(string message)
        {
            _messageQueue.Enqueue(message);
        }
    }
}
