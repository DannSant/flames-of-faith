using Game.Control;
using System;
using UnityEngine;

namespace Game.Utils
{
    /**
     * UpdateTimer is a utility class that manages a countdown timer for events.
     * It allows you to start an event with a specified duration, update the timer,
     * and trigger actions when the event starts and completes.
     */
    public class UpdateTimer 
    {
        private float eventDuration;
        private float eventTimer;
        private bool isEventActive=false;

        public event Action OnEventComplete;
        public event Action OnEventStarted;

        /**
         * Constructor to initialize the UpdateTimer with a specified event duration.
         * @param eventDuration The duration of the event in seconds.
         */
        public UpdateTimer(float eventDuration)
        {
            this.eventDuration = eventDuration;
            isEventActive = false;
        }
        /**
         * Starts the event with the specified duration and triggers the OnEventStarted action.
         *  
         *  **/
        public void StartEvent()
        {
            eventTimer = eventDuration;
            OnEventStarted?.Invoke();
            isEventActive = true;
        }

        /**
        * Updates the timer of the event and checks if the event has completed, if so calls the CompleteEvent method.
        *  This has to be called on the Update method of your mono behavior
        *  **/
        public void UpdateEvent()
        {
            if (eventTimer > 0)
            {
                eventTimer -= Time.deltaTime;
                if (eventTimer <= 0)
                {
                    CompleteEvent();
                }
            }
        }

        private void CompleteEvent()
        {
            OnEventComplete?.Invoke();
            isEventActive = false;
        }

        public bool GetIsEventActive()
        {
            return isEventActive;
        }
        public void SetEventDuration(float duration)
        {
            eventDuration = duration;
        }

        public float GetEventDuration()
        {
            return eventDuration;
        }

        public float GetEventTimer()
        {
            return eventTimer;
        }
    }

}