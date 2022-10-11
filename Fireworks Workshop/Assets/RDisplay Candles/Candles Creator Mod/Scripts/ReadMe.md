# Candle Creator Mod (Dynamic GUI)

This mod adds the ability to create candle racks, which are racks of roman candle fireworks commonly used by firework professionals.
Not only can users create a rack, but they can also save their created rack as a preset using the dynamic GUI and reload that preset into the rack betweeon
play sessions.

CandleCreator.cs is the main script that handles the overall GUI and saving and loading data using Json.net

CustomRackPanel.cs is a helper class which represents one "panel" or preset for candles

PanelData is a custom struct data type that contains all the information held by a CustomRackPanel Object
