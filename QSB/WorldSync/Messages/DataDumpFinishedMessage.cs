using QSB.Messaging;

namespace QSB.WorldSync.Messages;

public class DataDumpFinishedMessage : QSBMessage<string>
{
	public DataDumpFinishedMessage(string managerName) : base(managerName) => To = 0;

	public override void OnReceiveRemote() => HashErrorAnalysis.Instances[Data].AllDataSent(From);
}
