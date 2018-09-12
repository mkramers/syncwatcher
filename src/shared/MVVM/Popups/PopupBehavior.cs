namespace MVVM.Popups
{
    public enum PopupBehavior
    {
        SINGLE, //Only one instance of the same window/ viewmodel is shown
        SINGLE_NEW, //Always a new instance of the window/ viewmodel
        SINGLE_OFTYPE, //Only one instance of the same type of window is shown (Ex. only one report)
        MULTIPLE, //new window for everytime it is called
        BLOCKING
    }
}
