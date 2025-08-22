using System;
using UnityEngine;

namespace DoD.Tutorial
{
    public interface ITutorialStep
    {
        string Title { get; }
        string Instruction { get; }
        event Action<ITutorialStep> OnStepCompleted;
        void BeginStep();
        void EndStep();
    }

    /// <summary>
    /// 공통 구현을 제공하는 기본 클래스.
    /// </summary>
    public abstract class TutorialStepBase : MonoBehaviour, ITutorialStep
    {
        [SerializeField] private string title;
        [TextArea] [SerializeField] private string instruction;

        public string Title => this.title;
        public string Instruction => this.instruction;

        public event Action<ITutorialStep> OnStepCompleted;

        public virtual void BeginStep()
        {
            this.enabled = true;
            OnBeginInternal();
        }

        public virtual void EndStep()
        {
            OnEndInternal();
            this.enabled = false;
        }

        protected void Complete()
        {
            OnStepCompleted?.Invoke(this);
        }

        protected abstract void OnBeginInternal();
        protected abstract void OnEndInternal();
    }
}
