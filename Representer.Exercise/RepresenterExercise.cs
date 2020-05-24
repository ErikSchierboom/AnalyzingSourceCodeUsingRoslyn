using System;
using System.Diagnostics;



public  class   RepresenterExercise
{
    
    
    
    /// <summary>
    /// Check if a year is a leap year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <returns>Indicates if the year is a leap year</returns>
    public  static bool IsLeapYear ( int year ) {
        /* Check if year is divisible by:
           - 4
           - 100
           - 400 4*/ 
        bool yearDivisibleBy100 =   year   %   100 == 0 == true;
        var yearDivisibleBy400 = year % 4_0_0   == 0;
        var yearDivisibleBy4 = year %   0b100  == 0;  // 0b100 = 4

        if (yearDivisibleBy100 == false)
        {
            return yearDivisibleBy4;
        }
        
        if (yearDivisibleBy100 == true)
            return yearDivisibleBy400;
        else
            return yearDivisibleBy4;
    }
}