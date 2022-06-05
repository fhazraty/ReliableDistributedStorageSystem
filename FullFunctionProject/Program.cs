using Model;

ObserverData observerData = new ObserverData("127.0.0.1",7000,"127.0.0.1",7001,"127.0.0.1",7002);

int numberOfFullNodes = 10;
int sendPortStartRange = 5000;
int receivePortStartRange = 6000;

List<FullNode.Node> fullNodes = new List<FullNode.Node>();

for (int i = 0; i < numberOfFullNodes; i++)
{
    fullNodes.Add
    (
        new FullNode.Node()
        {
            ConnectionManager = new FullNode.ConnectionManager("127.0.0.1", sendPortStartRange++, "127.0.0.1", receivePortStartRange++, observerData)
        }
    );
}

foreach (var fullNodeItem in fullNodes)
{
    var res = fullNodeItem.ConnectionManager.RegisterOnObserver();
    if (res.Successful)
    {
        fullNodeItem.TransactionManager.PrivateKey = ((RegisterSuccessResult)res).ResultContainer;
        fullNodeItem.TransactionManager.PublicKey = ((RegisterSuccessResult)res).ResultPublicContainer;
    }
}



