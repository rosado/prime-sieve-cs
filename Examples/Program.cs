/*
 *  An example application using ChanLib. 
 *  =====================================
 *	It's just the example from Rob Pike's google talk about Newsqueak, 
 *	but rewritten in C# and using ChanLib (which provides a  very rough 
 *  equivalent of Newsqueak's channels).
 * 
 *	Date: 2007-11-26
 *	Modification: 2010-05-24 (use the Task library)
 *	Author: Roland Sadowski <szabla gmail com> http://www.haltingproblem.net
 *  License: Public domain
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using rosado.ChanLib;

namespace Examples
{
  class Program
  {
    static void Main(string[] args)
    {
      Channel<int> chan = Sieve();
      // print first 100 prime numbers 
      for (int i = 0; i < 100; i++)
      {
        Console.WriteLine("{0}", chan.Receive());
      }

      System.Environment.Exit(0);
    }

    /// <summary>
    /// Sends succesive positive integers (starting with 2) to a channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    static void Counter(System.Object channel)
    {
      var chan = channel as rosado.ChanLib.Channel<int>;
      int i = 2;
      for (; ; )
      {
        chan.Send(i++);
      }
    }

    /// <summary>
    /// Filters out the multiplies of a number passed in a member of filterObj.
    /// You should pass a FilterArg instance as a parameter.
    /// </summary>
    /// <param name="filterObj">The filter obj.</param>
    static void Filter(object filterObj)
    {
      FilterArg arg = filterObj as FilterArg;
      int i;
      for (; ; )
      {
        if (((i = arg.Listener.Receive()) % arg.Prime) != 0)
          arg.Sender.Send(i);
      }
    }

    /// <summary>
    /// Creates a prime sieve.
    /// </summary>
    /// <returns>prime sieve (a channel)</returns>
    static public Channel<int> Sieve()
    {
      var c = new Channel<int>();
      Task.Factory.StartNew(Counter, c);
      var prime = new Channel<int>();
      Task.Factory.StartNew(() =>
      {
        int p;
        Channel<int> newChan;
        for (; ; )
        {
          p = c.Receive();
          prime.Send(p);
          newChan = new Channel<int>();
          var arg = new FilterArg();
          arg.Prime = p;
          arg.Listener = c;
          arg.Sender = newChan;
          Task.Factory.StartNew(Filter, arg, TaskCreationOptions.LongRunning);
          c = newChan;
        }
      });
      return prime;
    }

    /// <summary>
    /// Helper class used to pass data to Filter method.
    /// </summary>
    class FilterArg
    {
      public int Prime;
      public Channel<int> Listener;
      public Channel<int> Sender;
    }
  }
}
