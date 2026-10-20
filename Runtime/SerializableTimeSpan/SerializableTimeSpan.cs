
using UnityEngine;
using System;

// TODO: Attribute to pick which fields should be visible.

namespace KOOP
{
	/// <summary>
	/// Serializable version of the <see cref="System.TimeSpan"/> class, with custom
	/// property drawer.
	/// </summary>
	[System.Serializable]
	public class SerializableTimeSpan
	{
		[SerializeField] 
		private long _ticks;
		
		public long Ticks => _ticks;

		public double TotalMilliseconds => _ticks / TimeSpan.TicksPerMillisecond;
		public double TotalSeconds => _ticks / TimeSpan.TicksPerSecond;
		public double TotalMinutes => _ticks / TimeSpan.TicksPerMinute;
		public double TotalHours => _ticks / TimeSpan.TicksPerHour;
		public double TotalDays => _ticks / TimeSpan.TicksPerDay;

    	public static implicit operator TimeSpan(SerializableTimeSpan d) => new(d._ticks);
	}
}