using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPServer.Interface
{
    public static class UnitConverter
    {
        //var that = this;

        //region -- convertDistance, distanceUnitAbbreviation
        /**
         * Converts distances from one unit to another.
         * @param {number} value The distance to convert.
         * @param {string} fromUnit A VRS.Distance unit to convert from.
         * @param {string} toUnit A VRS.Distance unit to convert to.
         * @returns {number} The converted value.
         */
        public static double ConvertDistance(double value, DistanceUnit fromUnit, DistanceUnit toUnit)
        {
            var result = value;

            if (fromUnit != toUnit)
            {
                switch (fromUnit)
                {
                    case DistanceUnit.Kilometres:
                        switch (toUnit)
                        {
                            case DistanceUnit.NauticalMiles: result *= 0.539956803; break;
                            case DistanceUnit.Miles: result *= 0.621371192; break;
                            default: throw new ArgumentException("Unknown distance unit " + toUnit);
                        }
                        break;
                    case DistanceUnit.NauticalMiles:
                        switch (toUnit)
                        {
                            case DistanceUnit.Kilometres: result *= 1.852; break;
                            case DistanceUnit.Miles: result *= 1.15078; break;
                            default: throw new ArgumentException("Unknown distance unit " + toUnit);
                        }
                        break;
                    case DistanceUnit.Miles:
                        switch (toUnit)
                        {
                            case DistanceUnit.Kilometres: result *= 1.609344; break;
                            case DistanceUnit.NauticalMiles: result *= 0.868976; break;
                            default: throw new ArgumentException("Unknown distance unit " + toUnit);
                        }
                        break;
                    default:
                        throw new ArgumentException("Unknown distance unit " + fromUnit);
                }
            }

            return result;
        }

        /**
         * Returns the translated abbreviation for a VRS.Distance unit.
         * @param {string} unit The VRS.Distance unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        /*this.distanceUnitAbbreviation = function(unit)
        {
            switch(unit) {
                case VRS.Distance.Kilometre:    return VRS.$$.KilometreAbbreviation;
                case VRS.Distance.NauticalMile: return VRS.$$.NauticalMileAbbreviation;
                case VRS.Distance.StatuteMile:  return VRS.$$.StatuteMileAbbreviation;
                default:                        throw 'Unknown distance unit ' + unit;
            }
        };*/
        //endregion

        //region -- convertHeight
        /**
         * Converts heights from one unit to another.
         * @param {number} value The height to convert.
         * @param {string} fromUnit A VRS.Height unit to convert from.
         * @param {string} toUnit A VRS.Height unit to convert to.
         * @returns {number} The converted value.
         */
        public static double ConvertHeight(double value, HeightUnit fromUnit, HeightUnit toUnit)
        {
            var result = value;

            if (fromUnit != toUnit)
            {
                switch (fromUnit)
                {
                    case HeightUnit.Feet:
                        switch (toUnit)
                        {
                            case HeightUnit.Metres: result *= 0.3048; break;
                            default: throw new ArgumentException("Unknown height unit " + toUnit);
                        }
                        break;
                    case HeightUnit.Metres:
                        switch (toUnit)
                        {
                            case HeightUnit.Feet: result *= 3.2808399; break;
                            default: throw new ArgumentException("Unknown height unit " + toUnit);
                        }
                        break;
                    default:
                        throw new ArgumentException("Unknown height unit " + fromUnit);
                }
            }

            return result;
        }

        /**
         * Returns the translated abbreviation for a VRS.Height unit.
         * @param {string} unit The VRS.Height unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        /*this.heightUnitAbbreviation = function(unit)
        {
            switch(unit) {
                case VRS.Height.Feet:           return VRS.$$.FeetAbbreviation;
                case VRS.Height.Metre:          return VRS.$$.MetreAbbreviation;
                default:                        throw 'Unknown height unit ' + unit;
            }
        };*/

        /**
         * Returns the translated abbreviation for a VRS.Height unit over time.
         * @param {string} unit The VRS.Height unit to get an abbreviation for.
         * @param {boolean} perSecond True if it is height over seconds, false if it is height over minutes.
         * @returns {string} The translated abbreviation.
         */
        /*this.heightUnitOverTimeAbbreviation = function(unit, perSecond)
        {
            if(perSecond) {
                switch(unit) {
                    case VRS.Height.Feet:       return VRS.$$.FeetPerSecondAbbreviation;
                    case VRS.Height.Metre:      return VRS.$$.MetrePerSecondAbbreviation;
                    default:                    throw 'Unknown height unit ' + unit;
                }
            } else {
                switch(unit) {
                    case VRS.Height.Feet:       return VRS.$$.FeetPerMinuteAbbreviation;
                    case VRS.Height.Metre:      return VRS.$$.MetrePerMinuteAbbreviation;
                    default:                    throw 'Unknown height unit ' + unit;
                }
            }
        };*/
        //endregion

        //region -- convertSpeed
        /**
         * Converts speeds from one unit to another.
         * @param {number} value The speed to convert.
         * @param {string} fromUnit A VRS.Speed unit to convert from.
         * @param {string} toUnit A VRS.Speed unit to convert to.
         * @returns {number} The converted value.
         */
        public static double ConvertSpeed(double value, SpeedUnit fromUnit, SpeedUnit toUnit)
        {
            var result = value;

            if (fromUnit != toUnit)
            {
                switch (fromUnit)
                {
                    case SpeedUnit.Knots:
                        switch (toUnit)
                        {
                            case SpeedUnit.KilometresPerHour: result *= 1.852; break;
                            case SpeedUnit.MilesPerHour: result *= 1.15078; break;
                            default: throw new ArgumentException("Unknown speed unit " + toUnit);
                        }
                        break;
                    case SpeedUnit.KilometresPerHour:
                        switch (toUnit)
                        {
                            case SpeedUnit.Knots: result *= 0.539957; break;
                            case SpeedUnit.MilesPerHour: result *= 0.621371; break;
                            default: throw new ArgumentException("Unknown speed unit " + toUnit);
                        }
                        break;
                    case SpeedUnit.MilesPerHour:
                        switch (toUnit)
                        {
                            case SpeedUnit.KilometresPerHour: result *= 1.60934; break;
                            case SpeedUnit.Knots: result *= 0.868976; break;
                            default: throw new ArgumentException("Unknown speed unit " + toUnit);
                        }
                        break;
                    default:
                        throw new ArgumentException("Unknown speed unit " + fromUnit);
                }
            }

            return result;
        }

        /**
         * Returns the translated abbreviation for a VRS.Speed unit.
         * @param {string} unit The VRS.Speed unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        /*this.speedUnitAbbreviation = function(unit)
        {
            switch(unit) {
                case VRS.Speed.Knots:               return VRS.$$.KnotsAbbreviation;
                case VRS.Speed.KilometresPerHour:   return VRS.$$.KilometresPerHourAbbreviation;
                case VRS.Speed.MilesPerHour:        return VRS.$$.MilesPerHourAbbreviation;
                default:                            throw 'Unknown speed unit ' + unit;
            }
        }*/
        //endregion

        //region convertVerticalSpeed
        /**
         * Converts a vertical speed from one unit to another.
         * @param {number}      verticalSpeed   The vertical speed in x units per minute to convert.
         * @param {VRS.Height}  fromUnit        The units that the vertical speed is expressed in.
         * @param {VRS.Height}  toUnit          The units to convert to.
         * @param {boolean}     perSecond       True if the vertical speed should be converted to y units per second.
         * @returns {*}
         */
        public static double ConvertVerticalSpeed(double verticalSpeed, HeightUnit fromUnit, HeightUnit toUnit, bool perSecond)
        {
            var result = verticalSpeed;

            if (fromUnit != toUnit) result = ConvertHeight(result, fromUnit, toUnit);
            if (perSecond) result = Math.Round(result / 60);


            return result;
        }
        //endregion

        //region --getPixelsOrPercent
        /**
         * Accepts an integer number of pixels or a string ending with '%' and returns an
         * object describing whether the value is pixels or percent, and a number indicating what that value is.
         * Percents are divided by 100 before being returned.
         * @param {String|Number} value Either the integer percentage or a string ending with '%'.
         * @returns {VRS_VALUE_PERCENT}
         */
        /*private getPixelsOrPercent (int value)
        {
            var valueAsString = String(value);
            var result = {
                value: parseInt(valueAsString),
                isPercent: VRS.stringUtility.endsWith(valueAsString, '%', false)
            };
            if(result.isPercent) result.value /= 100;

            return result;
        }*/
        //endregion
    }
}
