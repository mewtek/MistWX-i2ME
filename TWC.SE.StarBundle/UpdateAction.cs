namespace TWC.SE.StarBundle
{
    public class UpdateAction : FileAction
    {
        public UpdateAction()
        {
            this.FileActionType = "Update";
        }

        protected UpdateAction(UpdateAction action)
          : base((FileAction)action)
        {
        }

        public override FileAction Clone()
        {
            return (FileAction)new UpdateAction(this);
        }
    }
}
