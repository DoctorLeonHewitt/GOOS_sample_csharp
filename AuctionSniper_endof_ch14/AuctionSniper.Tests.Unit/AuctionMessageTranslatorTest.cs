using AuctionSniper.Domain;
using AuctionSniper.Fakes.XMPPServer;
using NUnit.Framework;
using Moq;

namespace AuctionSniper.Tests.Unit
{
    [TestFixture]
    public class AuctionMessageTranslatorTest
    {
        private const string SNIPER_ID = "test";
        public const Chat UNUSED_CHAT = null;
        private Mock<IAuctionEventListener> _mockListener;
        
        private AuctionMessageTranslator _translator;

        [SetUp]
        public void TestSetup()
        {
            _mockListener = new Mock<IAuctionEventListener>();
            _translator = new AuctionMessageTranslator(SNIPER_ID, _mockListener.Object);
        }
        

        [Test]
        public void NotifiesAuctionClosedWhenCloseMessageReceived()
        {
            var message = new Message(UNUSED_CHAT) {Body = "SOLVersion: 1.1; Event: CLOSE;"};
            var mlea = new MessageListenerEventArgs(message);
            _translator.InvokeProcessMessage(mlea);
            
            _mockListener.Verify(ml => ml.AuctionClosed());
        }

        [Test]
        public void NotifiesBidDetailsWhenCurrentPriceMessageReceivedFromSniper()
        {
            var message = new Message(UNUSED_CHAT)
                              {
                                  Body =
                                      string.Format(
                                      "SOLVersion: 1.1; Event: PRICE; CurrentPrice: 234; Increment: 5; Bidder: {0};",
                                      SNIPER_ID)
                              };
            var mlea = new MessageListenerEventArgs(message);
            _translator.InvokeProcessMessage(mlea);
            _mockListener.Verify(ml => ml.CurrentPrice(234, 5, Enums.PriceSource.FromSniper));
        }

        [Test]
        public void NotifiesBidDetailsWhenCurrentPriceMessageReceivedFromOtherBidder()
        {
            var message = new Message(UNUSED_CHAT)
                              {
                                  Body =
                                      "SOLVersion: 1.1; Event: PRICE; CurrentPrice: 192; Increment: 7; Bidder: Someone else;"
                              };
            var mlea = new MessageListenerEventArgs(message);
            _translator.InvokeProcessMessage(mlea);
            
            _mockListener.Verify(ml => ml.CurrentPrice(192, 7, Enums.PriceSource.FromOtherBidder));
        }
    }
}