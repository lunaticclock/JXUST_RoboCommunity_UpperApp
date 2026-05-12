using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UpperApp.UI
{
    internal class MapTracker
    {
        private readonly Canvas _canvas;
        private float _calibratedDistance = 1.0f;
        private float _aspectRatio = 1.0f;
        private Point? _startPoint;
        private Point? _endPoint;
        private readonly List<Polyline> _tracks = new();
        private Polyline _currentTrack;
        private double _lastX = -1, _lastY = -1;

        public string StartPoint => _startPoint.HasValue ? $"{_startPoint.Value.X:F0},{_startPoint.Value.Y:F0}" : "0,0";
        public string EndPoint => _endPoint.HasValue ? $"{_endPoint.Value.X:F0},{_endPoint.Value.Y:F0}" : "0,0";

        public MapTracker(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void SetCalibratedDistance(float distance)
        {
            _calibratedDistance = distance;
        }

        public void SetAspectRatio(float ratio)
        {
            _aspectRatio = ratio;
        }

        public void OnDistanceChanged(string distText, string yawText, Action<string> logCallback)
        {
            if (!float.TryParse(distText, out float dist)) return;
            if (!float.TryParse(yawText, out float yaw)) return;

            double rad = yaw * Math.PI / 180.0;
            double dx = dist * Math.Sin(rad) * _aspectRatio;
            double dy = -dist * Math.Cos(rad);

            if (_lastX < 0)
            {
                _lastX = _canvas.ActualWidth / 2;
                _lastY = _canvas.ActualHeight / 2;
            }

            double newX = _lastX + dx;
            double newY = _lastY + dy;

            if (_currentTrack == null)
            {
                _currentTrack = new Polyline
                {
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 2
                };
                _currentTrack.Points.Add(new Point(_lastX, _lastY));
                _canvas.Children.Add(_currentTrack);
                _tracks.Add(_currentTrack);
            }

            _currentTrack.Points.Add(new Point(newX, newY));

            var dot = new Ellipse { Width = 4, Height = 4, Fill = Brushes.Orange };
            Canvas.SetLeft(dot, newX - 2);
            Canvas.SetTop(dot, newY - 2);
            _canvas.Children.Add(dot);

            _lastX = newX;
            _lastY = newY;
        }

        public void SetBackgroundImage(ImageSource source)
        {
            _canvas.Background = new ImageBrush(source);
        }

        public void OnMapClick(Point p, Action<string> logCallback)
        {
            if (!_startPoint.HasValue)
            {
                _startPoint = p;
                var dot = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Lime };
                Canvas.SetLeft(dot, p.X - 4);
                Canvas.SetTop(dot, p.Y - 4);
                _canvas.Children.Add(dot);
            }
            else
            {
                _endPoint = p;
                var dot = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Red };
                Canvas.SetLeft(dot, p.X - 4);
                Canvas.SetTop(dot, p.Y - 4);
                _canvas.Children.Add(dot);

                var line = new Line
                {
                    X1 = _startPoint.Value.X, Y1 = _startPoint.Value.Y,
                    X2 = _endPoint.Value.X, Y2 = _endPoint.Value.Y,
                    Stroke = Brushes.Yellow,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 2 }
                };
                _canvas.Children.Add(line);
            }
        }

        private Line _previewLine;

        public void OnMouseMove(Point p)
        {
            if (_startPoint.HasValue && !_endPoint.HasValue)
            {
                if (_previewLine == null)
                {
                    _previewLine = new Line
                    {
                        Stroke = Brushes.Yellow,
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 4, 2 },
                        Opacity = 0.5
                    };
                    _canvas.Children.Add(_previewLine);
                }
                _previewLine.X1 = _startPoint.Value.X;
                _previewLine.Y1 = _startPoint.Value.Y;
                _previewLine.X2 = p.X;
                _previewLine.Y2 = p.Y;
            }
        }

        public string GetMousePosition(Point p)
        {
            return $"{p.X:F0},{p.Y:F0}";
        }

        public void Clear()
        {
            _canvas.Children.Clear();
            _tracks.Clear();
            _currentTrack = null;
            _previewLine = null;
            _startPoint = null;
            _endPoint = null;
            _lastX = -1;
            _lastY = -1;
        }
    }
}
