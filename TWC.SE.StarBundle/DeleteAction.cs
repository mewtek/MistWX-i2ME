namespace TWC.SE.StarBundle
{
    public class DeleteAction : FileAction
    {
        public DeleteAction()
        {
            this.FileActionType = "Delete";
        }

        protected DeleteAction(DeleteAction action)
          : base((FileAction)action)
        {
        }

        public override FileAction Clone()
        {
            return (FileAction)new DeleteAction(this);
        }
    }
}