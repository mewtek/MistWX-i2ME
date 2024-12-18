namespace TWC.SE.StarBundle
{
    public class AddAction : FileAction
    {
        public AddAction()
        {
            this.FileActionType = "Add";
        }

        protected AddAction(AddAction action)
          : base((FileAction)action)
        {
        }

        public override FileAction Clone()
        {
            return (FileAction)new AddAction(this);
        }
    }
}