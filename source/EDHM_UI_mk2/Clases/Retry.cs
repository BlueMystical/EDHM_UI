using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EDHM_UI_Patcher
{
	/// <summary>Se utiliza para hacer una tarea y si falla, reintentarla X veces.
	///	Retry.On<Exception>().For(3).With(context => { //Hacer algo aqui });
	/// 
	/// </summary>
	public class Retry
	{
		public Retry()
		{
			Conditions = new Collection<RetryCondition>();
		}

		public static RetryCondition On(Func<RetryConditionHandle, bool> predicate)
		{
			var retry = new Retry();
			return new RetryCondition(retry, predicate);
		}

		private static RetryCondition OnInternal<TException>(Retry retry) where TException : Exception
		{
			return OnInternal<TException>(retry, (handle) => true);
		}

		private static RetryCondition OnInternal<TException>(Retry retry, Func<RetryConditionHandle, bool> predicate) where TException : Exception
		{
			Func<RetryConditionHandle, bool> typeCheckingPredicate = (handle) => handle.Context.DidExceptionOnLastRun && handle.Context.LastException is TException && predicate(handle);
			return new RetryCondition(retry, typeCheckingPredicate);
		}

		public static RetryCondition On<TException>() where TException : Exception
		{
			var retry = new Retry();
			return OnInternal<TException>(retry);
		}

		public static RetryCondition On<TException>(Func<RetryConditionHandle, bool> predicate) where TException : Exception
		{
			var retry = new Retry();
			return OnInternal<TException>(retry, predicate);
		}

		public RetryCondition AndOn<TException>() where TException : Exception
		{
			return OnInternal<TException>(this);
		}

		public RetryCondition AndOn<TException>(Func<RetryConditionHandle, bool> predicate) where TException : Exception
		{
			return OnInternal<TException>(this, predicate);
		}

		public RetryResult<TOutput> With<TOutput>(Func<RetryContext, TOutput> target)
		{
			TOutput output = default(TOutput);
			Action<RetryContext> capturedTarget = (context) => output = target(context);
			var result = With(capturedTarget);
			var resultWithValue = result.WithValue(output);
			return resultWithValue;
		}

		public RetryResult With(Action<RetryContext> target)
		{
			var context = new RetryContext(this);

			do
			{
				context.DidExceptionOnLastRun = false;

				try
				{
					target(context);
					var anyConditionsMet = CheckAndUpdateFilterConditions(context);

					if (!anyConditionsMet)
					{
						return new RetryResult(context);
					}
				}
				catch (Exception ex)
				{
					context.DidExceptionOnLastRun = true;
					context.Exceptions.Push(ex);

					var anyConditionsMet = CheckAndUpdateFilterConditions(context);
					if (!anyConditionsMet)
					{
						throw ex;
					}
				}
			} while (context.KeepRetrying);

			throw new RetryException(context);
		}

		private bool CheckAndUpdateFilterConditions(RetryContext context)
		{
			var handlesForFilterConditionsMet = context.FilteredConditionHandles.Where(handle => handle.Condition.FilterCondition(handle));

			if (handlesForFilterConditionsMet.Count() > 0)
			{
				foreach (var handle in handlesForFilterConditionsMet)
				{
					handle.Occurences = handle.Occurences + 1;

					if (handle.Occurences == 1)
					{
						handle.FirstOccured = DateTimeOffset.Now;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public Collection<RetryCondition> Conditions { get; private set; }
	}

	public class RetryCondition
	{
		public Retry Retry { get; set; }

		public RetryCondition(Retry retry, Func<RetryConditionHandle, bool> predicate)
		{
			Retry = retry;
			Retry.Conditions.Add(this);
			FilterCondition = predicate;
		}

		public Retry For(uint times)
		{
			TerminationCondition = (handle) => handle.Occurences > times;
			return Retry;
		}

		public Retry For(TimeSpan duration)
		{
			TerminationCondition = (handle) =>
			{
				var durationSinceFirstOccured = DateTimeOffset.Now - handle.FirstOccured;
				var result = durationSinceFirstOccured > duration;
				return result;
			};

			return Retry;
		}

		public Retry Until(Func<RetryConditionHandle, bool> predicate)
		{
			TerminationCondition = predicate;
			return Retry;
		}

		public Retry Indefinitely()
		{
			TerminationCondition = (handle) => false;
			return Retry;
		}

		public Func<RetryConditionHandle, bool> FilterCondition { get; private set; }
		public Func<RetryConditionHandle, bool> TerminationCondition { get; private set; }
	}

	public class RetryConditionHandle
	{
		public RetryConditionHandle(RetryContext context, RetryCondition condition)
		{
			Context = context;
			Condition = condition;
		}

		public DateTimeOffset FirstOccured { get; internal set; }
		public uint Occurences { get; internal set; }

		public RetryContext Context { get; private set; }
		public RetryCondition Condition { get; private set; }
	}

	public class RetryContext
	{
		public RetryContext(Retry retry)
		{
			Retry = retry;
			ConditionHandles = retry.Conditions.Select(condition => new RetryConditionHandle(this, condition)).ToList();
		}

		public IEnumerable<RetryConditionHandle> ConditionHandles { get; private set; }

		public IEnumerable<RetryConditionHandle> FilteredConditionHandles
		{
			get
			{
				return ConditionHandles.Where(handle => handle.Condition.FilterCondition(handle));
			}
		}

		public IEnumerable<RetryConditionHandle> TerminatingConditionHandles
		{
			get
			{
				return FilteredConditionHandles.Where(handle => handle.Condition.TerminationCondition(handle));
			}
		}

		public Retry Retry { get; private set; }

		private Stack<Exception> m_Exceptions = null;

		public Stack<Exception> Exceptions
		{
			get
			{
				if (m_Exceptions == null)
				{
					m_Exceptions = new Stack<Exception>();
				}

				return m_Exceptions;
			}
		}

		public bool DidExceptionOnLastRun { get; set; }

		public Exception LastException
		{
			get
			{
				return Exceptions.Peek();
			}
		}

		public bool KeepRetrying
		{
			get
			{
				var handles = TerminatingConditionHandles.ToList();
				var any = handles.Any(handle => handle.Condition.TerminationCondition(handle));
				return any != true;
			}
		}

		public DateTimeOffset Started { get; private set; }
	}

	public class RetryException : Exception
	{
		private const string ExceptionMessage = "An error occured performing an operation. The operation we retried '{0}' times and failed, the last exception message was '{1}'. Check the inner exception for details.";

		public RetryException(RetryContext context)
			: base(string.Format(ExceptionMessage, context.Exceptions.Count, context.LastException.Message), context.LastException)
		{
			Context = context;
		}

		public RetryContext Context { get; private set; }
	}

	public class RetryResult
	{
		public RetryResult(RetryContext context)
		{
			Context = context;
		}

		public RetryContext Context { get; private set; }

		public RetryResult<TResult> WithValue<TResult>(TResult value)
		{
			var result = new RetryResult<TResult>(Context, value);
			return result;
		}
	}

	public class RetryResult<TResult> : RetryResult
	{
		public RetryResult(RetryContext context, TResult value) : base(context)
		{
			Value = value;
		}

		public TResult Value { get; private set; }
	}
}
