Lightning Bolt 2D - by Andrii Sudyn

A geometry-based electric charge or lightning effects for your 2D Unity projects.

Online manual: http://ax23w4.com/devlog/lightningbolt2d

To start using Lightning Bolt 2D click Game Object > Effects > Lightning Bolt 2D in Unity's main menu. 
Or do the same by right-clicking the Hierarchy panel. This will add a new Lightning Bolt 2D object to your scene. 

It will generate a demo lighning right away and it will work if you run the game. 

Now you can modify start and end position of the lightning by dragging the points. You can also configure some
parameters to fine tune how the lightning looks including its colors, width, segment count and how it animates
when the game is running.

The object is easily accessible through code. When you have a reference to it, you can start and stop the 
playback of the effects by setting isPlaying property like this:

LightningBolt2D.isPlaying=true;
LightningBolt2D.isPlaying=false;

There's also a method that allows to fire one (or as many as you've set in "Arc count" setting) lightnings
at once and let them play out without keeping continuosly generating new ones:

LightningBolt2D.fireOnce();

You can change positions of start and end points like this:

LightningBolt2D.startPoint=new Vector2(-8f,5f);
LightningBolt2D.endPoint.y=-6f;

You can change any of the object's setting in runtime like this:

LightningBolt2D.arcCount=3;
LightningBolt2D.pointCount=20;
LightningBolt2D.lineColor=Color.white;

For all the variables you can change refer to LightningBolt2D.cs

That's it. if you'll have questions, suggestions or found a bug, please contact me. 
I usually respond within a day or two.

Thanks for buying this asset and if you like it, be sure to give it a rating on the Asset Store.
This will help others find it and help me to keep updating it.

My twitter: @ax23w4
Email: andrii.sudyn@gmail.com
My other assets: https://ax23w4.com/devlog/assets/