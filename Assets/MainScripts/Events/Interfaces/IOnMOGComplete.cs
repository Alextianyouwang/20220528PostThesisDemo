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
public interface IOnEventEnter 
{
    public void QuickExecuteOnEnterEvent(string eventName);
}
public interface IOnEventExit
{
public void QuickExecuteOnExitEvent(string eventName);
}