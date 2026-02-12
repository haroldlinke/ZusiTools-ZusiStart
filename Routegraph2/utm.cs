// This file is part of utm.

// (c) Copyright 2019 Charles Taylor, Alexander Hajnal, Miguel Aguiar, FSchn
//
// Original Javascript by Charles Taylor:
// 	http://home.hiwaay.net/~taylorc/toolbox/geography/geoutm.html
// Port to C++ by Alexander Hajnal:
// 	http://alephnull.net/software/gis/UTM/UTM.h
// This C version was adapted from the C++ version by Miguel Aguiar, C# by FSchn
//
// This program is free software: you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

public static class UTM
{
	// Ellipsoid model constants (actual values here are for WGS84)
	private const double sm_a = 6378137.0;
	private const double sm_b = 6356752.314;
	// private const double sm_ecc_squared = 6.69437999013e-03;

	private const double utm_scale_factor = 0.9996;

	private static double deg_to_rad(double deg) { return (deg / 180.0 * System.Math.PI); }

	private static double rad_to_deg(double rad) { return (rad / System.Math.PI * 180.0); }

	// Computes the ellipsoidal distance from the equator to a point at a
	// given latitude.
	//
	// Reference:
	// 	Hoffmann-Wellenhof, B., Lichtenegger, H., and Collins, J.,
	// 	GPS: Theory and Practice, 3rd ed.  New York: Springer-Verlag Wien, 1994.
	//
	// Inputs:
	// 	phi	Latitude of the point, in radians.
	//
	// Globals:
	// 	sm_a	Ellipsoid model major axis.
	// 	sm_b	Ellipsoid model minor axis.
	//
	// Returns:
	// 	The ellipsoidal distance of the point from the equator, in meters.
	private static double arc_length_of_meridian(double phi)
	{
		/* Precalculate n */
		double n = (sm_a - sm_b) / (sm_a + sm_b);

		/* Precalculate alpha */
		double alpha = ((sm_a + sm_b) / 2.0) *
					 (1.0 + (System.Math.Pow(n, 2.0) / 4.0) + (System.Math.Pow(n, 4.0) / 64.0));

		/* Precalculate beta */
		double beta = (-3.0 * n / 2.0) + (9.0 * System.Math.Pow(n, 3.0) / 16.0) +
					(-3.0 * System.Math.Pow(n, 5.0) / 32.0);

		/* Precalculate gamma */
		double gamma =
			(15.0 * System.Math.Pow(n, 2.0) / 16.0) + (-15.0 * System.Math.Pow(n, 4.0) / 32.0);

		/* Precalculate delta */
		double delta =
			(-35.0 * System.Math.Pow(n, 3.0) / 48.0) + (105.0 * System.Math.Pow(n, 5.0) / 256.0);

		/* Precalculate epsilon */
		double epsilon = (315.0 * System.Math.Pow(n, 4.0) / 512.0);

		/* Now calculate the sum of the series and return */
		return alpha *
			   (phi + (beta * System.Math.Sin(2.0 * phi)) + (gamma * System.Math.Sin(4.0 * phi)) +
			(delta * System.Math.Sin(6.0 * phi)) + (epsilon * System.Math.Sin(8.0 * phi)));
	}

	// Determines the central meridian for the given UTM zone.
	//
	// Inputs:
	// 	zone	An integer value designating the UTM zone, range [1,60].
	//
	// Returns:
	// 	The central meridian for the given UTM zone, in radians
	// 	Range of the central meridian is the radian equivalent of [-177,+177].
	private static double utm_central_meridian(int zone)
	{
		return deg_to_rad(-183.0 + (double)(6 * zone));
	}

	// Computes the footpoint latitude for use in converting transverse
	// Mercator coordinates to ellipsoidal coordinates.
	//
	// Reference:
	// 	Hoffmann-Wellenhof, B., Lichtenegger, H., and Collins, J.,
	// 	GPS: Theory and Practice, 3rd ed.  New York: Springer-Verlag Wien, 1994.
	//
	// Inputs:
	// 	y	The UTM northing coordinate, in meters.
	//
	// Returns:
	// 	The footpoint latitude, in radians.
	private static double footpoint_latitude(double y)
	{
		/* Precalculate n (Eq. 10.18) */
		double n = (sm_a - sm_b) / (sm_a + sm_b);

		/* Precalculate alpha_ (Eq. 10.22) */
		/* (Same as alpha in Eq. 10.17) */
		double alpha_ = ((sm_a + sm_b) / 2.0) *
					  (1 + (System.Math.Pow(n, 2.0) / 4) + (System.Math.Pow(n, 4.0) / 64));

		/* Precalculate y_ (Eq. 10.23) */
		double y_ = y / alpha_;

		/* Precalculate beta_ (Eq. 10.22) */
		double beta_ = (3.0 * n / 2.0) + (-27.0 * System.Math.Pow(n, 3.0) / 32.0) +
					 (269.0 * System.Math.Pow(n, 5.0) / 512.0);

		/* Precalculate gamma_ (Eq. 10.22) */
		double gamma_ =
			(21.0 * System.Math.Pow(n, 2.0) / 16.0) + (-55.0 * System.Math.Pow(n, 4.0) / 32.0);

		/* Precalculate delta_ (Eq. 10.22) */
		double delta_ =
			(151.0 * System.Math.Pow(n, 3.0) / 96.0) + (-417.0 * System.Math.Pow(n, 5.0) / 128.0);

		/* Precalculate epsilon_ (Eq. 10.22) */
		double epsilon_ = (1097.0 * System.Math.Pow(n, 4.0) / 512.0);

		/* Now calculate the sum of the series (Eq. 10.21) */
		return y_ + (beta_ * System.Math.Sin(2.0 * y_)) + (gamma_ * System.Math.Sin(4.0 * y_)) +
			   (delta_ * System.Math.Sin(6.0 * y_)) + (epsilon_ * System.Math.Sin(8.0 * y_));
	}

	// Converts a latitude/longitude pair to x and y coordinates in the
	// Transverse Mercator projection.  Note that Transverse Mercator is not
	// the same as UTM; a scale factor is required to convert between them.
	//
	// Reference:
	// 	Hoffmann-Wellenhof, B., Lichtenegger, H., and Collins, J.,
	// 	GPS: Theory and Practice, 3rd ed.  New York: Springer-Verlag Wien, 1994.
	//
	// Inputs:
	// 	phi	Latitude of the point, in radians.
	// 	lambda	Longitude of the point, in radians.
	// 	lambda0	Longitude of the central meridian to be used, in radians.
	//
	// Outputs:
	// 	x	The x coordinate of the computed point.
	// 	y	The y coordinate of the computed point.
	//
	// Returns:
	// 	The function does not return a value.
	private static void map_lat_lon_to_xy(
		double phi, double lambda, double lambda0, out double x, out double y)
	{
		/* Precalculate ep2 */
		double ep2 = (System.Math.Pow(sm_a, 2.0) - System.Math.Pow(sm_b, 2.0)) / System.Math.Pow(sm_b, 2.0);

		/* Precalculate nu2 */
		double nu2 = ep2 * System.Math.Pow(System.Math.Cos(phi), 2.0);

		/* Precalculate N */
		double N = System.Math.Pow(sm_a, 2.0) / (sm_b * System.Math.Sqrt(1 + nu2));

		/* Precalculate t */
		double t = System.Math.Tan(phi);
		double t2 = t * t;

		/* Precalculate l */
		double l = lambda - lambda0;

		/* Precalculate coefficients for l**n in the equations below
		   so a normal human being can read the expressions for easting
		   and northing
		   -- l**1 and l**2 have coefficients of 1.0 */
		double l3coef = 1.0 - t2 + nu2;

		double l4coef = 5.0 - t2 + 9 * nu2 + 4.0 * (nu2 * nu2);

		double l5coef =
			5.0 - 18.0 * t2 + (t2 * t2) + 14.0 * nu2 - 58.0 * t2 * nu2;

		double l6coef =
			61.0 - 58.0 * t2 + (t2 * t2) + 270.0 * nu2 - 330.0 * t2 * nu2;

		double l7coef =
			61.0 - 479.0 * t2 + 179.0 * (t2 * t2) - (t2 * t2 * t2);

		double l8coef =
			1385.0 - 3111.0 * t2 + 543.0 * (t2 * t2) - (t2 * t2 * t2);

		/* Calculate easting (x) */
		x = N * System.Math.Cos(phi) * l +
			 (N / 6.0 * System.Math.Pow(System.Math.Cos(phi), 3.0) * l3coef * System.Math.Pow(l, 3.0)) +
			 (N / 120.0 * System.Math.Pow(System.Math.Cos(phi), 5.0) * l5coef * System.Math.Pow(l, 5.0)) +
			 (N / 5040.0 * System.Math.Pow(System.Math.Cos(phi), 7.0) * l7coef * System.Math.Pow(l, 7.0));

		/* Calculate northing (y) */
		y = arc_length_of_meridian(phi) +
			 (t / 2.0 * N * System.Math.Pow(System.Math.Cos(phi), 2.0) * System.Math.Pow(l, 2.0)) +
			 (t / 24.0 * N * System.Math.Pow(System.Math.Cos(phi), 4.0) * l4coef * System.Math.Pow(l, 4.0)) +
			 (t / 720.0 * N * System.Math.Pow(System.Math.Cos(phi), 6.0) * l6coef * System.Math.Pow(l, 6.0)) +
			 (t / 40320.0 * N * System.Math.Pow(System.Math.Cos(phi), 8.0) * l8coef * System.Math.Pow(l, 8.0));
	}

	// Converts x and y coordinates in the Transverse Mercator projection to
	// a latitude/longitude pair.  Note that Transverse Mercator is not
	// the same as UTM; a scale factor is required to convert between them.
	//
	// Reference:
	// 	Hoffmann-Wellenhof, B., Lichtenegger, H., and Collins, J.,
	// 	GPS: Theory and Practice, 3rd ed.  New York: Springer-Verlag Wien, 1994.
	//
	// Inputs:
	// 	x	The easting of the point, in meters.
	// 	y	The northing of the point, in meters.
	// 	lambda0	Longitude of the central meridian to be used, in radians.
	//
	// Outputs:
	// 	phi	Latitude in radians.
	// 	lambda	Longitude in radians.
	//
	// Returns:
	// 	The function does not return a value.
	//
	// Remarks:
	// 	The local variables Nf, nuf2, tf, and tf2 serve the same purpose as
	// 	N, nu2, t, and t2 in MapLatLonToXY, but they are computed with respect
	// 	to the footpoint latitude phif.
	//
	// 	x1frac, x2frac, x2poly, x3poly, etc. are to enhance readability and
	// 	to optimize computations.
	private static void map_xy_to_lat_lon(
		double x, double y, double lambda0, out double phi, out double lambda)
	{
		/* Get the value of phif, the footpoint latitude. */
		double phif = footpoint_latitude(y);

		/* Precalculate ep2 */
		double ep2 = (System.Math.Pow(sm_a, 2.0) - System.Math.Pow(sm_b, 2.0)) / System.Math.Pow(sm_b, 2.0);

		/* Precalculate cos (phif) */
		double cf = System.Math.Cos(phif);

		/* Precalculate nuf2 */
		double nuf2 = ep2 * System.Math.Pow(cf, 2.0);

		/* Precalculate Nf and initialize Nfpow */
		double Nf = System.Math.Pow(sm_a, 2.0) / (sm_b * System.Math.Sqrt(1 + nuf2));
		double Nfpow = Nf;

		/* Precalculate tf */
		double tf = System.Math.Tan(phif);
		double tf2 = tf * tf;
		double tf4 = tf2 * tf2;

		/* Precalculate fractional coefficients for x**n in the equations
		   below to simplify the expressions for latitude and longitude. */
		double x1frac = 1.0 / (Nfpow * cf);

		Nfpow *= Nf; /* now equals Nf**2) */
		double x2frac = tf / (2.0 * Nfpow);

		Nfpow *= Nf; /* now equals Nf**3) */
		double x3frac = 1.0 / (6.0 * Nfpow * cf);

		Nfpow *= Nf; /* now equals Nf**4) */
		double x4frac = tf / (24.0 * Nfpow);

		Nfpow *= Nf; /* now equals Nf**5) */
		double x5frac = 1.0 / (120.0 * Nfpow * cf);

		Nfpow *= Nf; /* now equals Nf**6) */
		double x6frac = tf / (720.0 * Nfpow);

		Nfpow *= Nf; /* now equals Nf**7) */
		double x7frac = 1.0 / (5040.0 * Nfpow * cf);

		Nfpow *= Nf; /* now equals Nf**8) */
		double x8frac = tf / (40320.0 * Nfpow);

		/* Precalculate polynomial coefficients for x**n.
		   -- x**1 does not have a polynomial coefficient. */
		double x2poly = -1.0 - nuf2;

		double x3poly = -1.0 - 2 * tf2 - nuf2;

		double x4poly = 5.0 + 3.0 * tf2 + 6.0 * nuf2 - 6.0 * tf2 * nuf2 -
					  3.0 * (nuf2 * nuf2) - 9.0 * tf2 * (nuf2 * nuf2);

		double x5poly =
			5.0 + 28.0 * tf2 + 24.0 * tf4 + 6.0 * nuf2 + 8.0 * tf2 * nuf2;

		double x6poly =
			-61.0 - 90.0 * tf2 - 45.0 * tf4 - 107.0 * nuf2 + 162.0 * tf2 * nuf2;

		double x7poly =
			-61.0 - 662.0 * tf2 - 1320.0 * tf4 - 720.0 * (tf4 * tf2);

		double x8poly =
			1385.0 + 3633.0 * tf2 + 4095.0 * tf4 + 1575 * (tf4 * tf2);

		/* Calculate latitude */
		phi = phif + x2frac * x2poly * (x * x) +
			   x4frac * x4poly * System.Math.Pow(x, 4.0) + x6frac * x6poly * System.Math.Pow(x, 6.0) +
			   x8frac * x8poly * System.Math.Pow(x, 8.0);

		/* Calculate longitude */
		lambda = lambda0 + x1frac * x + x3frac * x3poly * System.Math.Pow(x, 3.0) +
			  x5frac * x5poly * System.Math.Pow(x, 5.0) + x7frac * x7poly * System.Math.Pow(x, 7.0);
	}

	// Converts a latitude/longitude pair to x and y coordinates in the
	// Universal Transverse Mercator projection.
	//
	// Inputs:
	// 	lat	Latitude of the point, in degrees.
	// 	lon	Longitude of the point, in degrees.
	// 	zone	UTM zone to be used for calculating values for x and y.
	// 		If zone is null, the routine will determine the appropriate zone
	// 		from the value of lon.
	//
	// Outputs:
	// 	x	The x coordinate (easting) of the computed point. (in meters)
	// 	y	The y coordinate (northing) of the computed point. (in meters)
	//
	// Returns:
	// 	The UTM zone
	public static int LatLonToUtm(
		double lat, double lon, int? zone, out double x, out double y)
	{
		int zone_;

		if (zone == null)
			zone_ = (int)System.Math.Floor((lon + 180.0) / 6.0) + 1;
		else
			zone_ = zone.Value;

		if (zone_ < 1 || zone_ > 60)
			throw new System.ArgumentOutOfRangeException("zone");

		map_lat_lon_to_xy(deg_to_rad(lat),
				  deg_to_rad(lon),
				  utm_central_meridian(zone_),
				  out x,
				  out y);

		/* Adjust easting and northing for UTM system. */
		x = x * utm_scale_factor + 500000.0;
		y = y * utm_scale_factor;

		if (y < 0.0)
			y = y + 10000000.0;

		return zone_;
	}

	// Converts x and y coordinates in the Universal Transverse Mercator
	// projection to a latitude/longitude pair.
	//
	// Inputs:
	// 	x		The easting of the point, in meters.
	// 	y		The northing of the point, in meters.
	// 	zone		The UTM zone in which the point lies.
	// 	southhemi	Greater than zero if the point is in the south
	// 			hemisphere.
	//
	// Outputs:
	// 	lat	The latitude of the point, in degrees.
	// 	lon	The longitude of the point, in degrees.
	public static void UtmToLatLon(
		double x, double y, int zone, int southhemi, out double lat, out double lon)
	{
		x -= 500000.0;
		x /= utm_scale_factor;

		/* If in southern hemisphere, adjust y accordingly. */
		if (southhemi > 0)
			y -= 10000000.0;

		y /= utm_scale_factor;

		double cmeridian = utm_central_meridian(zone);
		map_xy_to_lat_lon(x, y, cmeridian, out lat, out lon);

		lat = rad_to_deg(lat);
		lon = rad_to_deg(lon);
	}
	
  // Version2 of Simple UTM to Lat/Lon conversion (WGS84) provided by CoPilot AI
  // zoneNumber: e.g. 32 for Germany
  // northernHemisphere: true if north of equator
  public static (double lat, double lon) UtmToLatLon2(double easting, double northing, int zoneNumber, bool northernHemisphere = true)
  {
    const double a = 6378137.0; // WGS84 major axis
    const double f = 1 / 298.257223563; // WGS84 flattening
    const double k0 = 0.9996;

    double e = Math.Sqrt(f * (2 - f));
    double e1sq = e * e / (1 - e * e);

    double x = easting - 500000.0; // remove 500,000 meter offset
    double y = northing;
    if (!northernHemisphere) y -= 10000000.0;

    double lonOrigin = (zoneNumber - 1) * 6 - 180 + 3;

    double m = y / k0;
    double mu = m / (a * (1 - e * e / 4.0 - 3 * e * e * e * e / 64.0 - 5 * e * e * e * e * e * e / 256.0));

    double phi1Rad = mu
        + (3 * e / 2 - 27 * Math.Pow(e, 3) / 32.0) * Math.Sin(2 * mu)
        + (21 * e * e / 16 - 55 * Math.Pow(e, 4) / 32.0) * Math.Sin(4 * mu)
        + (151 * Math.Pow(e, 3) / 96.0) * Math.Sin(6 * mu);

    double n1 = a / Math.Sqrt(1 - e * e * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
    double t1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
    double c1 = e1sq * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
    double r1 = a * (1 - e * e) / Math.Pow(1 - e * e * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
    double d = x / (n1 * k0);

    double lat = phi1Rad - (n1 * Math.Tan(phi1Rad) / r1) *
        (d * d / 2 - (5 + 3 * t1 + 10 * c1 - 4 * c1 * c1 - 9 * e1sq) * Math.Pow(d, 4) / 24
        + (61 + 90 * t1 + 298 * c1 + 45 * t1 * t1 - 252 * e1sq - 3 * c1 * c1) * Math.Pow(d, 6) / 720);

    lat = lat * 180.0 / Math.PI;

    double lon = lonOrigin + (d - (1 + 2 * t1 + c1) * Math.Pow(d, 3) / 6
        + (5 - 2 * c1 + 28 * t1 - 3 * c1 * c1 + 8 * e1sq + 24 * t1 * t1) * Math.Pow(d, 5) / 120) / Math.Cos(phi1Rad);

    lon = lon * 180.0 / Math.PI;

    return (lat, lon);
  }
}

