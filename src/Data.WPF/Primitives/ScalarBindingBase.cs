﻿using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ScalarBindingBase<T> : ScalarBinding
        where T : UIElement, new()
    {
        public Input<T> Input { get; private set; }

        public void SetInput(Input<T> input, Action<T> flushAction)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (input.Binding != null)
                throw new ArgumentException(Strings.Binding_InputAlreadyInitialized, nameof(input));
            if (flushAction == null)
                throw new ArgumentNullException(nameof(flushAction));
            VerifyNotSealed();
            input.Initialize(this, flushAction);
            Input = input;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            if (Input != null)
                Input.Flush((T)element);
        }

        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        internal sealed override UIElement Setup()
        {
            var element = CachedList.GetOrCreate(ref _cachedElements, Create);
            Setup(element);
            Refresh(element);
            return element;
        }

        protected abstract void Setup(T element);

        protected abstract void Refresh(T element);

        protected abstract void Cleanup(T element);

        internal sealed override void Refresh(UIElement element)
        {
            Refresh((T)element);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var e = (T)element;
            Cleanup(e);
            CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
