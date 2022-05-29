
public interface IOnMOGComplete 
{
    public void MarchObjectGroupFormationCompletionCallBack(MarchManager.MarchObjectGroup currentMOG);

}

public interface IOnSceneStart 
{
    public void StartScene();
}

public interface IOnSetMOGFeatures 
{
    public void SetMarchObjectGroupFeatures();
}

public interface IOnSetActivation 
{
    public EventExecuter.ObjectActivationState WillMarchObjectBeActivated(MarchManager.MarchObjectGroup currentMOG);
}


public interface IOnEventTrigger 
{
    public void QuickExecuteOnEventTriggered(string eventName);
}