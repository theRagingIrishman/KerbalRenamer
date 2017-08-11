DOCUMENTATION

Did you get here alright?  Double-click worked?  Good.

First: THERE IS NO SUPPORT FOR THIS MOD SO PISS OFF.

Here's how this program works:

This mod renames every Kerbal the game generates based on a config file in the
base directory (KerbalRenamer.cfg).  The example config file comes with some
Russian names randomly scraped from Wikipedia; I don't know if it generates
proper Russian names but it works well enough for my ignorant American ass.
Also, because it renames /every/ Kerbal you won't get Jeb, Bill, Bob, or
Valentina Kerman unless you configure the mod.

The options in the config file are as follows:

preserveOriginals : If set to true will preserve Jeb, Bill, Bob, and
    Valentina as the starting available crew. Defaults to false because I'm
	sick of these assholes.
generateNewStats : If true will generate new stats for all Kerbals, including
    their profession.  Defaults to true.
femalePercent : This is the chance to generate a female Kerbal anytime a new
    Kerbal is generated.  Possible values are from 0.0 to 1.0.  Defaults to
	0.5.  4Chan rejoice.
badassPercent : The chance of generating a badass out of any given Kerbal.
    Possible values are from 0.0 to 1.0.  Defaults to 0.05.
useBellCurveMethod : If set to true, uses a (5d21 - 5) * 0.01 method to
    generate Kerbal Courage and Stupidity.  Otherwise generates a random number
    between 0.0 and 1.0.  This is intended to generate more average Kerbals
	more often.  Defaults to true.
dontInsultMe : If set to true along with useBellCurveMethod, only rolls 3d21
    for Stupidity because astronauts~.  Defaults to false.

You can alter the way the mod generates names by editing the KerbalRenamer.cfg
file.  You are allowed three "syllables" per name which the mod will string
together to make the actual string.  You can omit "syllables" entirely if you
want, and you may even be allowed to use empty keys, but I haven't tried and I
don't really care if it works that way or not.  You can increase the odds of a
certain syllable being chosen by entering it multiple times; that's pretty much
the only way to weight it and because this is KSP (and not, say, Dwarf
Fortress) that's about as far as I'm going to take it.

You can also generate female surnames by defining FLASTNAME1, FLASTNAME2, and
FLASTNAME3 nodes in the same manner as LASTNAME# nodes.  If the plugin cannot
find any of those nodes it will use LASTNAME# nodes for your female Kerbals as
normal.  Thanks to Volodyuka on the KSP forums for pointing out that Cyrillic
surnames will be gendered.

Here is a very simple config setup to generate some silly names.  Try playing
around with this one if you're wondering how to make this mod work for you.

KERBALRENAMER
{
    preserveOriginals = false
	generateNewStats = true
    femalePercent = 0.5
    badassPercent = 0.05
    useBellCurveMethod = true
    dontInsultMe = false

	FFIRSTNAME1
	{
        key = F
        key = P
    }
	FFIRSTNAME2
	{
        key = ar
        key = or
    }
	FFIRSTNAME3
	{
        key = i
        key = a
        key = y
        key = e
    }
	MFIRSTNAME1
	{
        key = T
        key = D
    }
	MFIRSTNAME2
	{
        key = ew
        key = oc
    }
	MFIRSTNAME3
	{
        key = o
        key = u
    }
	LASTNAME1
	{
        key = R
        key = K
        key = B
    }
	LASTNAME2
	{
        key = ip
        key = ug
    }
	LASTNAME3
	{
        key = o
        key = a
    }
}
