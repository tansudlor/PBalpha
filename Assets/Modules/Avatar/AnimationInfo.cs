namespace com.playbux.avatar
{
    public class AnimationInfo : IAnimationInfo
    {
        private ClipName name;
        private float speed;
        private PlayAction action; //stop , play , other 

        public AnimationInfo(ClipName name, float speed, PlayAction action)
        {
            this.speed = speed;
            this.name = name;
            this.action = action;
        }

        public PlayAction GetAnimationAction()
        {
            return action;
        }

        public ClipName GetAnimationName()
        {
            return name;
        }

        public float GetAnimationSpeed()
        {
            return speed;
        }

    }
}