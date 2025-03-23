using System;
using System.Collections.Generic;

public class Player
{
    public int Level { get; set; }

    public string Name { get; set; }

    public string Image { get; set; }

    public Player(int level, string name, string image)
    {
        Level = level;
        Name = name;
        Image = image;
    }

    public string ShortName()
    {
        var a = Name.Split(" ");
        var c = Math.Min(a[1].Length, 4);
        return $"{a[0][..2]}.{a[1][..c]}";
    }

    public override string ToString()
    {
        return $"Level {Level}: {Name} ({Image})";
    }

    public static Dictionary<string, Player> GetPlayerMap()
    {
        // Dictionary to hold all players by their ID (p1 to p38)
        Dictionary<string, Player> players = new Dictionary<string, Player>()
        {
            // Base Players (level 0)
            { "L0P1", new Player(0, "Lumen Vale", "p1") },
            { "L0P2", new Player(0, "Frost Wraith", "p2") },
            { "L0P3", new Player(0, "Echo Night", "p3") },
            { "L0P4", new Player(0, "Shade Drift", "p4") },

            // Level 1
            { "L1P1", new Player(1, "Cedric Cox", "p5") },
            { "L1P2", new Player(1, "Zorin Vein", "p6") },
            { "L1P3", new Player(1, "Valery Skye", "p7") },
            { "L1P4", new Player(1, "Sable Noir", "p8") },

            // Level 2
            { "L2P1", new Player(2, "Raven Flux", "p9") },
            { "L2P2", new Player(2, "Dorian Ash", "p10") },
            { "L2P3", new Player(2, "Draven Shade", "p11") },
            { "L2P4", new Player(2, "Morgana Fyre", "p12") },

            // Level 3
            { "L3P1", new Player(3, "Lucian Doom", "p13") },
            { "L3P2", new Player(3, "Rex Mortis", "p14") },
            { "L3P3", new Player(3, "Viktor Gloom", "p15") },
            { "L3P4", new Player(3, "Damien Crypt", "p16") },

            // Level 4
            { "L4P1", new Player(4, "Ezekiel Black", "p17") },
            { "L4P2", new Player(4, "Malachi Vex", "p18") },
            { "L4P3", new Player(4, "Ophelia Sable", "p19") },
            { "L4P4", new Player(4, "Unknown Beast", "ptemp") },

            // Level 5
            { "L5P1", new Player(5, "Octavia Grave", "p20") },
            { "L5P2", new Player(5, "Gideon Vex", "p21") },
            { "L5P3", new Player(5, "Mordecai Grim", "p22") },
            { "L5P4", new Player(5, "Unknown Beast", "ptemp") },

            // Level 6
            { "L6P1", new Player(6, "Selene Rift", "p23") },
            { "L6P2", new Player(6, "Draco Fume", "p24") },
            { "L6P3", new Player(6, "Lilith Crow", "p25") },
            { "L6P4", new Player(6, "Unknown Beast", "ptemp") },

            // Level 7
            { "L7P1", new Player(7, "Lucius Thorn", "p26") },
            { "L7P2", new Player(7, "Ophelia Bane", "p27") },
            { "L7P3", new Player(7, "Raven Mort", "p28") },
            { "L7P4", new Player(7, "Unknown Beast", "ptemp") },

            // Level 8
            { "L8P1", new Player(8, "Xander Bleak", "p29") },
            { "L8P2", new Player(8, "Vesper Nyx", "p30") },
            { "L8P3", new Player(8, "Unknown Beast", "ptemp") },
            { "L8P4", new Player(8, "Unknown Beast", "ptemp") },

            // Level 9
            { "L9P1", new Player(9, "Cassius Void", "p31") },
            { "L9P2", new Player(9, "Aldric Maul", "p32") },
            { "L9P3", new Player(9, "Unknown Beast", "ptemp") },
            { "L9P4", new Player(9, "Unknown Beast", "ptemp") },

            // Level 10
            { "L10P1", new Player(10, "Nerissa Wrath", "p33") },
            { "L10P2", new Player(10, "Quentin Vile", "p34") },
            { "L10P3", new Player(10, "Unknown Beast", "ptemp") },
            { "L10P4", new Player(10, "Unknown Beast", "ptemp") },

            // Level 11
            { "L11P1", new Player(11, "Dominic Abyss", "p35") },
            { "L11P2", new Player(11, "Lavinia Dusk", "p36") },
            { "L11P3", new Player(11, "Unknown Beast", "ptemp") },
            { "L11P4", new Player(11, "Unknown Beast", "ptemp") },

            // Level 12
            { "L12P1", new Player(12, "Octavian Raze", "p37") },
            { "L12P2", new Player(12, "Unknown Beast", "ptemp") },
            { "L12P3", new Player(12, "Unknown Beast", "ptemp") },
            { "L12P4", new Player(12, "Unknown Beast", "ptemp") },

            // Level 13
            { "L13P1", new Player(13, "Unknown Beast", "ptemp") },
            { "L13P2", new Player(13, "Unknown Beast", "ptemp") },
            { "L13P3", new Player(13, "Unknown Beast", "ptemp") },
            { "L13P4", new Player(13, "Unknown Beast", "ptemp") },

            // Level 14
            { "L14P1", new Player(14, "Unknown Beast", "ptemp") },
            { "L14P2", new Player(14, "Unknown Beast", "ptemp") },
            { "L14P3", new Player(14, "Unknown Beast", "ptemp") },
            { "L14P4", new Player(14, "Unknown Beast", "ptemp") },

            // Level 15
            { "L15P1", new Player(15, "Unknown Beast", "ptemp") },
            { "L15P2", new Player(15, "Unknown Beast", "ptemp") },
            { "L15P3", new Player(15, "Unknown Beast", "ptemp") },
            { "L15P4", new Player(15, "Unknown Beast", "ptemp") },

            // Level 16
            { "L16P1", new Player(16, "Unknown Beast", "ptemp") },
            { "L16P2", new Player(16, "Unknown Beast", "ptemp") },
            { "L16P3", new Player(16, "Unknown Beast", "ptemp") },
            { "L16P4", new Player(16, "Unknown Beast", "ptemp") },
        };

        return players;
    }
}