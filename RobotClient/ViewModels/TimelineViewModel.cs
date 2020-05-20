﻿using System.Windows.Media.Animation;
using RobotInterface.Timeline;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Diagnostics;
using Caliburn.Micro;
using System.Windows;
using System;
using RobotInterface.Helpers;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static RobotInterface.Helpers.ResizerHelper;

namespace RobotInterface.ViewModels
{
    public class TimelineViewModel : Screen, IHandle<Clock>
    {
        public TimelineViewModel(
            IEventAggregator eventAggregator)
        {
            // Event Aggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            DialogEventAggregatorProvider.EA.Subscribe(this);

            // Timeline initialisatiors
            FloaterPos = 0;
            TimeDuration();
            FeedTimelines();
        }

        #region Private Members

        // EventAggregator
        private IEventAggregator _eventAggregator { get; }

        private const int GridCanvasOffset = 72;

        #endregion

        #region Timeline

        #region Timeline Properties

        private BindableCollection<TimeLine> _Timelines = new BindableCollection<TimeLine>();
        private TimeSpan totalTime = new TimeSpan(0, 0, 10);
        private Rectangle _SelectedEventRectangle;
        private string _RectangleChangeType = "";
        private TimeSpan _MouseMovedInSeconds;
        private string _CurrentTimeStr = "0";
        private bool _MouseStatus = false;
        private TimeLine _SelectedTimeLine;
        private double _MouseDistanceMoved;
        private double _MouseCurrentPosX;
        private Duration _StoryDuration;
        private bool _MouseClickedEvent;
        public int TimelineWidth = 650;
        private double _GridMousePosX;
        private string _CurrentTime;
        private string _DebugString;
        private int _FloaterPos = 0;
        private int _MousePosX = 0;
        private int TLEventIdx = 0;
        private Point LastPoint;

        private HitType MouseHitType { get; set; }
        public bool LeftButtonDown { get; set; }
        public int TimeLineIdx { get; set; }
        public int TimeLineEventIdx { get; set; }
        public TimeSpan mouseDownStartTime { get; set; }
        public double mouseDownX { get; set; }
        public double mouseDownRightX { get; set; }
        private double PixelsPerSecond { get; set; }

        /// <summary>
        /// FLoater position
        /// </summary>
        public int FloaterPos
        {
            get { return _FloaterPos; }
            set
            {
                _FloaterPos = value;
                GetCurrentTimeStr();
                NotifyOfPropertyChange(() => FloaterPos);
            }
        }

        /// <summary>
        /// Mouse click status
        /// </summary>
        public bool MouseStatus
        {
            get { return _MouseStatus; }
            set => Set(ref _MouseStatus, value);
        }

        /// <summary>
        /// Position of the mouse within canvas (actually goes out of bound to the right but dont know why)
        /// </summary>
        public int MousePosX
        {
            get { return _MousePosX; }
            set => Set(ref _MousePosX, value);
        }

        /// <summary>
        /// Current time string
        /// </summary>
        public string CurrentTimeStr
        {
            get { return _CurrentTimeStr; }
            set => Set(ref _CurrentTimeStr, value);
        }

        /// <summary>
        /// Duration of a story 
        /// </summary>
        public Duration StoryDuration
        {
            get { return _StoryDuration; }
            set => Set(ref _StoryDuration, value);
        }


        /// <summary>
        /// Current time 
        /// </summary>
        public string CurrentTime
        {
            get { return _CurrentTime; }
            set
            {
                _CurrentTime = value;
                NotifyOfPropertyChange(() => CurrentTime);
            }
        }

        /// <summary>
        /// Debug string initialisation
        /// </summary>
        public string DebugString
        {
            get { return _DebugString; }
            set => Set(ref _DebugString, value);
        }

        private double _TimeLineEventPosX;

        /// <summary>
        /// Timeline event pos x
        /// </summary>
        public double TimeLineEventPosX
        {
            get { return _TimeLineEventPosX; }
            set => Set(ref _TimeLineEventPosX, value);
        }

        private TimeSpan _TimelineStart;

        /// <summary>
        /// Timeline start 
        /// </summary>
        public TimeSpan TimelineStart
        {
            get { return _TimelineStart; }
            set => Set(ref _TimelineStart, value);
        }

        private TimeSpan _TimelineDuration;

        /// <summary>
        /// Timeline duration
        /// </summary>
        public TimeSpan TimelineDuration
        {
            get { return _TimelineDuration; }
            set { _TimelineDuration = value; }
        }

        /// <summary>
        /// List of timelines
        /// </summary>
        public BindableCollection<TimeLine> Timelines
        {
            get { return _Timelines; }
            set { _Timelines = value; }
        }

        private TimeLineEvent _SelectedTimeLineEvent;

        /// <summary>
        /// Selected timeline event
        /// </summary>
        public TimeLineEvent SelectedTimeLineEvent
        {
            get { return _SelectedTimeLineEvent; }
            set => Set(ref _SelectedTimeLineEvent, value);
        }

        /// <summary>
        /// Selected timeline
        /// </summary>
        public TimeLine SelectedTimeLine
        {
            get { return _SelectedTimeLine; }
            set => Set(ref _SelectedTimeLine, value);
        }

        /// <summary>
        /// Mouse moved in seconds
        /// </summary>
        public TimeSpan MouseMovedInSeconds
        {
            get { return _MouseMovedInSeconds; }
            set => Set(ref _MouseMovedInSeconds, value);
        }

        /// <summary>
        /// Mouse distance moved in px
        /// </summary>
        public double MouseDistanceMoved
        {
            get { return _MouseDistanceMoved; }
            set => Set(ref _MouseDistanceMoved, value);
        }

        /// <summary>
        /// Mouse clicked
        /// </summary>
        public bool MouseClickedEvent
        {
            get { return _MouseClickedEvent; }
            set => Set(ref _MouseClickedEvent, value);
        }

        /// <summary>
        /// Current mouse position
        /// </summary>
        public double MouseCurrentPosX
        {
            get { return _MouseCurrentPosX; }
            set => Set(ref _MouseCurrentPosX, value);
        }


        /// <summary>
        /// Mouse position in the grid
        /// </summary>
        public double GridMousePosX
        {
            get { return _GridMousePosX; }
            set => Set(ref _GridMousePosX, value);
        }

        /// <summary>
        /// Selected event rectangle
        /// </summary>
        public Rectangle SelectedEventRectangle
        {
            get { return _SelectedEventRectangle; }
            set => Set(ref _SelectedEventRectangle, value);
        }


        /// <summary>
        /// Rectangle change type - is it move or resize
        /// </summary>
        public string RectangleChangeType
        {
            get { return _RectangleChangeType; }
            set => Set(ref _RectangleChangeType, value);
        }

        #endregion

        #region Timeline Methods

        #region Floater

        /// <summary>
        /// Mouse Move Event within canvas
        /// </summary>
        /// <param name="source"></param>
        public void CanvasMouseMove(Canvas source)
        {
            Point p = Mouse.GetPosition(source);
            MousePosX = Convert.ToInt32(p.X);

            if (MouseStatus)
            {
                if (MousePosX > TimelineWidth)
                    MousePosX = TimelineWidth;
                if (MousePosX < 0)
                    MousePosX = 0;

                FloaterPos = MousePosX;
                TimeDuration();
            }
        }

        /// <summary>
        /// Gets seconds out of position of the Floater
        /// </summary>
        /// <returns></returns>
        private void TimeDuration()
        {
            double totTime = totalTime.TotalMilliseconds;
            if (FloaterPos != 0)
            {
                totTime -= (totTime * FloaterPos / TimelineWidth);
            }

            StoryDuration = new Duration(TimeSpan.FromMilliseconds(totTime));
        }

        /// <summary>
        /// Get current time str
        /// </summary>
        private void GetCurrentTimeStr()
        {
            int totTime = (int)totalTime.TotalMilliseconds;
            TimeSpan curTime = new TimeSpan();
            if (FloaterPos != 0)
            {
                curTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(totTime * FloaterPos / TimelineWidth));
                CurrentTimeStr = curTime.ToString();
            }
            else if (FloaterPos == 0)
            {
                curTime = new TimeSpan(0, 0, 0, 0, 0);
                CurrentTimeStr = curTime.ToString();
            }
        }

        /// <summary>
        /// Stop button click event
        /// </summary>
        public void StopButton()
        {
            FloaterPos = 0;
            TimeDuration();
        }

        /// <summary>
        /// Mouse Down event inside canvas
        /// </summary>
        public void MouseDown()
        {
            MouseStatus = true;
        }

        /// <summary>
        /// Mouse release
        /// </summary>
        public void MouseLeftUp()
        {
            MouseStatus = false;
            LeftButtonDown = false;
            RectangleChangeType = "";
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        #endregion

        #region Timeline Mouse Events

        // <summary>
        /// Left button down on timeline event (rectangle)
        /// </summary>
        /// <param name="rect"></param>
        public void TimeLineEventLeftDown(object source)
        {
            // Checks if the clicked thing is a rectangle and sets it as suchs
            if (!(source is Rectangle selectedRect))
                return;

            SelectedEventRectangle = selectedRect;
            LeftButtonDown = true;
            MouseClickedEvent = true;

            Grid rectParent = selectedRect.Parent as Grid;
            TimeLineEvent clickedEvent = selectedRect.DataContext as TimeLineEvent;

            //Gets the selected timeline event
            GetSelectedTimelineEvent(clickedEvent);

            LastPoint = Mouse.GetPosition(rectParent);
            mouseDownX = Mouse.GetPosition(rectParent).X;

            // Debug 
            TimeLineEventPosX = mouseDownX;
            DebugString = SelectedTimeLineEvent.Name;
            mouseDownStartTime = SelectedTimeLineEvent.Start;

            // Set the change type
            MouseHitType = SetHitType(selectedRect, rectParent);
            Mouse.OverrideCursor = ResizerHelper.SetMouseCursor(MouseHitType);

            if (MouseHitType == HitType.Left || MouseHitType == HitType.Right)
            {
                RectangleChangeType = "Resize";
            }
            else if (MouseHitType == HitType.Body)
            {
                RectangleChangeType = "Move";
            }
        }

        /// <summary>
        /// Right Mouse Button Down inside timeline canvas
        /// </summary>
        /// <param name="source"></param>
        public void MouseRightDown(object source)
        {
            if (!(source is Grid timelineGrid))
                return;

            mouseDownRightX = Mouse.GetPosition(timelineGrid).X;

            TimeLine clickedTimeline = timelineGrid.DataContext as TimeLine;
            GetSelectedTimeline(clickedTimeline);
        }


        /// <summary>
        /// Right button on timeline event
        /// </summary>
        /// <param name="source"></param>
        public void TimeLineEventRightDown(object source)
        {
            if (!(source is Rectangle selectedRect))
                return;

            TimeLineEvent clickedTimeLineEvent = selectedRect.DataContext as TimeLineEvent;
            GetSelectedTimelineEvent(clickedTimeLineEvent);

        }

        /// <summary>
        /// Move Mouse anywhere
        /// </summary>
        /// <param name="source"></param>
        public void TimelineMouseMove(object source)
        {
            if (!(source is Grid parent))
                return;

            double thisX = Mouse.GetPosition(parent).X - GridCanvasOffset;
            GridMousePosX = thisX;

            if (!LeftButtonDown)
                return;

            // Pixel / Seconds constant make width a variable TO-DO

            switch (RectangleChangeType)
            {
                case "Resize":
                    // See how much the mouse has moved.
                    double offset_x = thisX - LastPoint.X;
                    double marginLeft = SelectedEventRectangle.Margin.Left;

                    // Get the rectangle's current position.
                    double new_x = marginLeft;
                    double new_width = SelectedEventRectangle.Width;

                    // Update the rectangle.
                    switch (MouseHitType)
                    {
                        case HitType.Left:
                            new_x += offset_x;
                            new_width -= offset_x;
                            break;
                        case HitType.Right:
                            new_width += offset_x;
                            break;
                    }

                    // Don't use negative width or height.
                    if ((new_width > 0))
                    {
                        //double distanceMoved = thisX - mouseDownX;
                        TimeSpan newStart = TimeSpan.FromSeconds(new_x / PixelsPerSecond);
                        TimeSpan newDuration = TimeSpan.FromSeconds(new_width / PixelsPerSecond);

                        // Update the rectangle.
                        SelectedEventRectangle.Width = new_width;
                        SelectedEventRectangle.Margin = new Thickness(new_x);

                        // Checking if Resizing of event would go outside of timeline scope 
                        if (newStart < new TimeSpan(0, 0, 0))
                            newStart = new TimeSpan(0, 0, 0);
                        else if (Timelines[TimeLineIdx].Events[TimeLineEventIdx].Start + newDuration > Timelines[TimeLineIdx].Duration)
                            newDuration = Timelines[TimeLineIdx].Events[TimeLineEventIdx].Duration;
                        else if (newDuration < new TimeSpan(0, 0, 0, 1)) // Event can't be shorter than 1 second
                            newDuration = Timelines[TimeLineIdx].Events[TimeLineEventIdx].Duration;
                        else
                            Timelines[TimeLineIdx].Events[TimeLineEventIdx].Duration = newDuration;

                        // Save the mouse's new location.
                        LastPoint.X = thisX;
                    }
                    break;
                case "Move":
                    MouseCurrentPosX = thisX;
                    double distanceMoved = thisX - mouseDownX;

                    TimeSpan timeMoved = TimeSpan.FromSeconds(distanceMoved / PixelsPerSecond);

                    MouseMovedInSeconds = timeMoved;
                    //SelectedTimeLineEvent.Start = mouseDownStartTime + timeMoved;
                    MouseDistanceMoved = distanceMoved;

                    // new local variable
                    var newEventPos = mouseDownStartTime + timeMoved;

                    // Check if start time is less than zero
                    if (newEventPos < new TimeSpan(0, 0, 0)) newEventPos = new TimeSpan(0, 0, 0);

                    // Check if move would push the event out of timeline
                    if (newEventPos + Timelines[TimeLineIdx].Events[TimeLineEventIdx].Duration > Timelines[TimeLineIdx].Duration)
                        newEventPos = Timelines[TimeLineIdx].Duration - Timelines[TimeLineIdx].Events[TimeLineEventIdx].Duration;

                    // Set new start time
                    Timelines[TimeLineIdx].Events[TimeLineEventIdx].Start = newEventPos;
                    break;
                default:
                    return;
            }

            Timelines.Refresh();
        }

        /// <summary>
        /// When mouse leaves rectangle area
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="args"></param>
        public void MouseRectangleLeave(object rect)
        {
            if (LeftButtonDown)
                return;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        /// <summary>
        /// Mouse enter rectangle handles mouse pointer
        /// </summary>
        /// <param name="rect"></param>s
        public void MouseRectangleEnter(object rect)
        {
            if (!(rect is Rectangle selectedRect))
                return;

            if (RectangleChangeType == "Resize")
                return;

            Grid rectParent = selectedRect.Parent as Grid;

            MouseHitType = SetHitType(selectedRect, rectParent);
            Mouse.OverrideCursor = SetMouseCursor(MouseHitType);
        }

        /// <summary>
        /// Mouse Move handles cursor change
        /// </summary>
        /// <param name="rect"></param>
        public void MouseRectangleMove(object rect)
        {
            if (!(rect is Rectangle selectedRect))
                return;

            if (RectangleChangeType == "Resize")
                return;

            Grid rectParent = selectedRect.Parent as Grid;

            MouseHitType = SetHitType(selectedRect, rectParent);
            Mouse.OverrideCursor = SetMouseCursor(MouseHitType);
        }

        #endregion

        #region Timeline Control

        /// <summary>
        /// Method for feeding the timeline with dummy data
        /// </summary>
        public void FeedTimelines()
        {
            Timelines.Clear();
            TimeLine first = new TimeLine
            {
                Name = "Focus"
            };
            first.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 1), Duration = new TimeSpan(0, 0, 2), Name = "Vskok1" });
            //first.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 4), Duration = new TimeSpan(0, 0, 5), Name = "Vskok2" });
            //first.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 13), Duration = new TimeSpan(0, 0, 3), Name = "Vskok3" });
            Timelines.Add(first);

            TimeLine second = new TimeLine
            {
                Name = "Zoom"
            };
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 2), Duration = new TimeSpan(0, 0, 3), Name = "Visje1" });
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 6), Duration = new TimeSpan(0, 0, 1), Name = "Visje2" });
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 0, 10, 5), Duration = new TimeSpan(0, 0, 0, 4, 5), Name = "Visje3" });
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 14), Duration = new TimeSpan(0, 0, 3), Name = "Visje4" });
            Timelines.Add(second);

            TimeLine third = new TimeLine
            {
                Name = "Outputs",
            };
            third.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 2), Duration = new TimeSpan(0, 0, 3), Name = "Buksy1" });
            third.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 7), Duration = new TimeSpan(0, 0, 1), Name = "Buksy2" });
            third.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 0, 8, 5), Duration = new TimeSpan(0, 0, 0, 2, 5), Name = "Buksy2" });
            third.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 16), Duration = new TimeSpan(0, 0, 3), Name = "Buksy2" });
            Timelines.Add(third);

            PixelsPerSecond = TimelineWidth / Timelines[TimeLineIdx].Duration.TotalSeconds;
        }

        /// <summary>
        /// Add new event to the timeline
        /// </summary>
        /// <param name="source"></param>
        public void AddEventToTimeLine(object source)
        {
            if (!(source is MenuItem timelineMenuItem))
                return;

            TimeSpan newStart = TimeSpan.FromSeconds(mouseDownRightX / PixelsPerSecond);

            TimeLine clickedTimeline = timelineMenuItem.DataContext as TimeLine;

            GetSelectedTimeline(clickedTimeline);
            Timelines[TimeLineIdx].Events.Add(
                new TimeLineEvent()
                {
                    Start = newStart,
                    Duration = new TimeSpan(0, 0, 0, 1, 500),
                    Name = $"Event {TLEventIdx}"
                });
            TLEventIdx++;
            Timelines.Refresh();
        }

        /// <summary>
        /// Remove the selected event from timeline
        /// </summary>
        /// <param name="source"></param>
        public void RemoveEventFromTimeLine(object source)
        {
            Timelines[TimeLineIdx].Events.RemoveAt(TimeLineEventIdx);
        }

        #endregion

        #region Timeline Method Helpers

        /// <summary>
        /// Gets the indexes of the selected event in the selected timeline
        /// </summary>
        private void GetSelectedTimelineEvent(TimeLineEvent clickedEvent)
        {
            // Finding the selected timeline event 
            foreach (var timeline in Timelines)
            {
                foreach (var tlEvent in timeline.Events)
                {
                    if (tlEvent == clickedEvent)
                    {
                        SelectedTimeLineEvent = tlEvent;
                        TimeLineIdx = Timelines.IndexOf(timeline);
                        TimeLineEventIdx = timeline.Events.IndexOf(tlEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the index of selected timeline
        /// </summary>
        private void GetSelectedTimeline(TimeLine clickedTimeline)
        {
            // Finding the selected timeline event 
            foreach (var timeline in Timelines)
            {
                if( timeline == clickedTimeline)
                {
                    SelectedTimeLine = timeline;
                    TimeLineIdx = Timelines.IndexOf(timeline);
                }
            }
        }

        #endregion

        #endregion

        #endregion



        #region Handlers

        /// <summary>
        /// CurrentTimeInvalidatedEventHandler's message
        /// </summary>
        /// <param name="message"></param>
        public void Handle(Clock message)
        {
            Debug.WriteLine($"Current time is: {message.CurrentTime}");
            CurrentTime = message.CurrentTime.ToString();
            var progress = message.CurrentProgress;
            Debug.WriteLine($"Progres: {progress * 100} % ");
            FloaterPos = Convert.ToInt32(progress * TimelineWidth);
        }
        #endregion

    }
}
