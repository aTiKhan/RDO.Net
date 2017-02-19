﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ScalarPane : ScalarBinding
    {
        private List<ScalarBinding> _bindings = new List<ScalarBinding>();
        private List<string> _names = new List<string>();

        public void AddChild<T>(ScalarBinding<T> binding, string name)
            where T : UIElement, new()
        {
            Binding.VerifyAdding(binding, nameof(binding));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            VerifyNotSealed();
            _bindings.Add(binding);
            _names.Add(name);
            binding.Seal(this, _bindings.Count - 1);
        }

        internal abstract Pane CreatePane();

        private Pane[] Create(int startOffset)
        {
            _settingUpStartOffset = startOffset;

            if (startOffset == FlowCount)
                return Array<Pane>.Empty;

            var count = FlowCount - startOffset;
            var result = new Pane[count];
            for (int i = 0; i < count; i++)
                result[i] = Create();
            return result;
        }

        private Pane Create()
        {
            var result = CreatePane().InitChildren(_bindings, _names);
            OnCreated(result);
            return result;
        }

        private int _settingUpStartOffset;
        private Pane[] _settingUpPanes;
        private IReadOnlyList<Pane> SettingUpPanes
        {
            get { return _settingUpPanes; }
        }

        private Pane SettingUpPane { get; set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            Debug.Assert(!Flowable);
            return SettingUpPane;
        }

        internal sealed override void BeginSetup(int startOffset)
        {
            if (Flowable)
            {
                _settingUpPanes = Create(startOffset);
                for (int i = 0; i < SettingUpPanes.Count; i++)
                    SettingUpPanes[i].BeginSetup(_bindings);
            }
            else if (startOffset == 0)
            {
                SettingUpPane = Create();
                SettingUpPane.BeginSetup(_bindings);
            }
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            Debug.Assert(!Flowable);
            SettingUpPane = value == null ? Create() : (Pane)value;
            SettingUpPane.BeginSetup(_bindings);
        }

        internal sealed override void PrepareSettingUpElement(int flowIndex)
        {
            if (Flowable)
            {
                Debug.Assert(SettingUpPanes != null);
                SettingUpPane = SettingUpPanes[flowIndex - _settingUpStartOffset];
            }
        }

        internal override void ClearSettingUpElement()
        {
            if (Flowable)
                SettingUpPane = null;
        }

        internal sealed override UIElement Setup(int flowIndex)
        {
            var result = SettingUpPane;
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(flowIndex);
            ExitSetup();
            return result;
        }

        internal sealed override void Refresh(UIElement element)
        {
            ((Pane)element).Refresh(_bindings);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var pane = (Pane)element;
            pane.Cleanup(_bindings);
        }

        internal sealed override void EndSetup()
        {
            for (int i = 0; i < SettingUpPanes.Count; i++)
                SettingUpPanes[i].EndSetup(_bindings);
            _settingUpPanes = null;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            ((Pane)element).FlushInput(_bindings);
        }
    }
}
