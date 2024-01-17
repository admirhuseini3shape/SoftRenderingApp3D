using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftRenderingApp3D.Factories
{
    public interface IFactory<out TInstance>
    {
        TInstance Create<TInput>(TInput input);

    }
    public interface ICanCreate<in TInput, TInstance>
    {
        TInstance Create(TInput input, IFactory<TInstance> factory);
    }
    public class Factory<TInstance> : IFactory<TInstance>
        where TInstance : class
    {
        private readonly Dictionary<Type, Func<object, TInstance>> _dictionary = 
            new Dictionary<Type, Func<object, TInstance>>();

        public TInstance Create<TInput>(TInput input)
        {
            var inputType = input.GetType();
            var hasMapping = _dictionary.Keys.Any(x => x.IsAssignableFrom(inputType));
            if(!hasMapping)
                throw new InvalidOperationException("Could not find constructor for given type!");

            var key = _dictionary.Keys.First(x => x.IsAssignableFrom(inputType));
            var instance = _dictionary[key](input);

            return instance ??
                   throw new InvalidOperationException("Object could not be instantiated!");
        }

        public void Add<TInput>(ICanCreate<TInput, TInstance> target)
        {
            if(_dictionary.TryGetValue(typeof(TInput), out _))
                return;

            _dictionary[typeof(TInput)] = (va) => target.Create((TInput)va, this);
        }
    }
}
