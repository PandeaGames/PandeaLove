using System;

public static class GenericFactoryUtility<T> where T : class 
{
    public static T Create()
    {
        T instance = (T)Activator.CreateInstance(typeof(T));

        return instance;
    }

    public static T Create(object[] args)
    {
        T instance = (T) Activator.CreateInstance(typeof(T), args);

        return instance;
    }
}