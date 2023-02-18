namespace Utils
{
	public class SingletonTracker
	{
		private object instance;

		public void Set(object instance)
		{
			if (this.instance != null)
			{
				throw new System.InvalidOperationException($"Double initialization of {instance.GetType().Name}");
			}
			this.instance = instance;
		}
	}
}
