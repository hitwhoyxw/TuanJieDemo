using System;

//public class Singleton<T> where T : class
//{
//    public static T Instance
//    {
//        get
//        {
//            return LazyHolder.instance;
//        }
//    }
//    public static class LazyHolder
//    {
//        public static readonly T instance = (T)Activator.CreateInstance(typeof(T), true);
//    }
//}

public class Singleton<T> where T : new()
{
    public static T Instance
    {
        get
        {
            return LazyHolder.instance;
        }
    }
    public static class LazyHolder
    {
        public static readonly T instance = new T();
    }
}