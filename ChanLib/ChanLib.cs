/*
 *  ChanLib is trying to emulate the idea of channel in the Newsqueak language by Rob Pike.
 *	At the same time it's an introduction to the world of multithreading.
 *	Date: 2007-11-26
 *	Author: Roland Sadowski <szabla gmail com> http://www.haltingproblem.net
 *  License: Public domain
 */

using System;
using System.Collections.Generic;
using System.Threading;

namespace rosado.ChanLib
{
  /// <summary>
  /// Channel for exchanging data between threads.
  /// Inspired by channels from the Newsqueak language.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Channel<T>
  {
    #region private

    EventWaitHandle sendIt;
    EventWaitHandle waitForData;
    T data;
    readonly object locker = new object();

    #endregion

    public Channel()
    {
      sendIt = new EventWaitHandle(false, EventResetMode.AutoReset);
      waitForData = new EventWaitHandle(false, EventResetMode.AutoReset);
    }

    public Channel(string handleName)
    {
      sendIt = new EventWaitHandle(false, EventResetMode.AutoReset, handleName);
      waitForData = new EventWaitHandle(false, EventResetMode.AutoReset);
    }

    #region Public Methods

    /// <summary>
    /// Sends the specified packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <returns></returns>
    public void Send(T packet)
    {
      waitForData.WaitOne();
      lock (locker) { data = packet; }
      sendIt.Set();
    }

    /// <summary>
    /// Receive data from the channel.
    /// </summary>
    /// <returns></returns>
    public T Receive()
    {
      T packet;
      waitForData.Set();
      sendIt.WaitOne();
      lock (locker)
      {
        packet = data;
        data = default(T);
      }
      return packet;
    }

    #endregion
  }
}
