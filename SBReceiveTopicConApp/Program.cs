using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace SBReceiveTopicConApp
{
    class Program
    {
        static NamespaceManager _namespaceManager;
        static void Main(string[] args)
        {
            CollectSBDetails();
            ReceiveMessage();
        }

        private static void ReceiveMessage()
        {
            TokenProvider tokenProvider = _namespaceManager.Settings.TokenProvider; ;

            if (_namespaceManager.TopicExists("DataCollectionTopic"))
            {
                MessagingFactory factory = MessagingFactory.Create(_namespaceManager.Address, tokenProvider);

                //Same as Queue
                //MessageReceiver receiver = factory.CreateMessageReceiver("DataCollectionTopic/subscriptions/Inventory");
                MessageReceiver receiver = factory.CreateMessageReceiver("DataCollectionTopic/subscriptions/Dashboard");

                BrokeredMessage receivedMessage = null;
                try
                {
                    while ((receivedMessage = receiver.Receive()) != null)
                    {
                        ProcessMessage(receivedMessage);
                        receivedMessage.Complete();
                    }
                    factory.Close();
                    //_namespaceManager.DeleteSubscription("DataCollectionTopic", "Inventory");
                    _namespaceManager.DeleteSubscription("DataCollectionTopic", "Dashboard");
                    _namespaceManager.DeleteTopic("DataCollectionTopic");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    receivedMessage.Abandon();
                }
            }

        }

        private static void ProcessMessage(BrokeredMessage receivedMessage)
        {
            Console.WriteLine("Label: {0}, MessageID: {1}, StoreName: {2}, MachineID: {3}", receivedMessage.Label, receivedMessage.MessageId, receivedMessage.Properties["StoreName"], receivedMessage.Properties["MachineID"]);
        }
        private static void CollectSBDetails()
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"].ToString());
        }
    }
}
