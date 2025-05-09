#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

using log4net.Core;
using log4net.Util;

namespace log4net.Filter;

/// <summary>
/// Simple filter to match a string an event property
/// </summary>
/// <remarks>
/// <para>
/// Simple filter to match a string in the value for a
/// specific event property
/// </para>
/// </remarks>
/// <author>Nicko Cadell</author>
public class PropertyFilter : StringMatchFilter
{
  /// <summary>
  /// The key to lookup in the event properties and then match against.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The key name to use to lookup in the properties map of the
  /// <see cref="LoggingEvent"/>. The match will be performed against 
  /// the value of this property if it exists.
  /// </para>
  /// </remarks>
  public string? Key { get; set; }

  /// <summary>
  /// Check if this filter should allow the event to be logged
  /// </summary>
  /// <param name="loggingEvent">the event being logged</param>
  /// <returns>see remarks</returns>
  /// <remarks>
  /// <para>
  /// The event property for the <see cref="Key"/> is matched against 
  /// the <see cref="StringMatchFilter.StringToMatch"/>.
  /// If the <see cref="StringMatchFilter.StringToMatch"/> occurs as a substring within
  /// the property value then a match will have occurred. If no match occurs
  /// this function will return <see cref="FilterDecision.Neutral"/>
  /// allowing other filters to check the event. If a match occurs then
  /// the value of <see cref="StringMatchFilter.AcceptOnMatch"/> is checked. If it is
  /// true then <see cref="FilterDecision.Accept"/> is returned otherwise
  /// <see cref="FilterDecision.Deny"/> is returned.
  /// </para>
  /// </remarks>
  public override FilterDecision Decide(LoggingEvent loggingEvent)
  {
    loggingEvent.EnsureNotNull();

    // Check if we have a key to lookup the event property value with
    if (Key is null)
    {
      // We cannot filter so allow the filter chain
      // to continue processing
      return FilterDecision.Neutral;
    }

    // Lookup the string to match in from the properties using 
    // the key specified.
    object? msgObj = loggingEvent.LookupProperty(Key);

    // Use an ObjectRenderer to convert the property value to a string
    string? msg = loggingEvent.Repository?.RendererMap.FindAndRender(msgObj);

    // Check if we have been setup to filter
    if (msg is null || (StringToMatch is null && m_regexToMatch is null))
    {
      // We cannot filter so allow the filter chain
      // to continue processing
      return FilterDecision.Neutral;
    }

    // Firstly check if we are matching using a regex
    if (m_regexToMatch is not null)
    {
      // Check the regex
      if (m_regexToMatch.Match(msg).Success == false)
      {
        // No match, continue processing
        return FilterDecision.Neutral;
      }

      // we've got a match
      return AcceptOnMatch ? FilterDecision.Accept : FilterDecision.Deny;
    }
    else if (StringToMatch is not null)
    {
      // Check substring match
      if (msg.IndexOf(StringToMatch) == -1)
      {
        // No match, continue processing
        return FilterDecision.Neutral;
      }

      // we've got a match
      return AcceptOnMatch ? FilterDecision.Accept : FilterDecision.Deny;
    }
    return FilterDecision.Neutral;
  }
}
