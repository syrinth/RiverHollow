<?xml version="1.0" encoding="UTF-8"?>
<tileset name="Spring" tilewidth="32" tileheight="32" tilecount="81" columns="9">
 <image source="Spring.png" trans="ffffff" width="288" height="288"/>
 <terraintypes>
  <terrain name="Grass" tile="10"/>
  <terrain name="Dirt" tile="14"/>
  <terrain name="Water" tile="15"/>
  <terrain name="Road" tile="37"/>
 </terraintypes>
 <tile id="0" terrain="1,1,1,0"/>
 <tile id="1" terrain="1,1,0,0"/>
 <tile id="2" terrain="1,1,0,1"/>
 <tile id="3" terrain="0,0,0,1"/>
 <tile id="4" terrain="0,0,1,0"/>
 <tile id="5" terrain="1,1,1,2">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="6" terrain="1,1,2,2">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="7" terrain="1,1,2,1">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="8" terrain="1,1,1,1"/>
 <tile id="9" terrain="1,0,1,0"/>
 <tile id="10" terrain="0,0,0,0"/>
 <tile id="11" terrain="0,1,0,1"/>
 <tile id="12" terrain="0,1,0,0"/>
 <tile id="13" terrain="1,0,0,0"/>
 <tile id="14" terrain="1,2,1,2">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="15" terrain="2,2,2,2">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="16" terrain="2,1,2,1">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="18" terrain="1,0,1,1"/>
 <tile id="19" terrain="0,0,1,1"/>
 <tile id="20" terrain="0,1,1,1"/>
 <tile id="21" terrain="2,2,2,1">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="22" terrain="2,2,1,2">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="23" terrain="1,2,1,1">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="24" terrain="2,2,1,1">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="25" terrain="2,1,1,1">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="27" terrain="1,1,1,3"/>
 <tile id="28" terrain="1,1,3,3"/>
 <tile id="29" terrain="1,1,3,1"/>
 <tile id="30" terrain="2,1,2,2">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
  <animation>
   <frame tileid="30" duration="1000"/>
   <frame tileid="48" duration="1000"/>
  </animation>
 </tile>
 <tile id="31" terrain="1,2,2,2">
  <properties>
   <property name="Impassable" type="bool" value="true"/>
  </properties>
  <animation>
   <frame tileid="31" duration="1000"/>
   <frame tileid="49" duration="1000"/>
  </animation>
 </tile>
 <tile id="36" terrain="1,3,1,3"/>
 <tile id="37" terrain="3,3,3,3"/>
 <tile id="38" terrain="3,1,3,1"/>
 <tile id="39" terrain="2,2,2,1"/>
 <tile id="40" terrain="2,2,1,2"/>
 <tile id="45" terrain="1,3,1,1"/>
 <tile id="46" terrain="3,3,1,1"/>
 <tile id="47" terrain="3,1,1,1"/>
 <tile id="48" terrain="2,1,2,2"/>
 <tile id="49" terrain="1,2,2,2"/>
 <tile id="54">
  <properties>
   <property name="Sleep" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="57" terrain="3,3,3,1"/>
 <tile id="58" terrain="3,3,1,3"/>
 <tile id="66" terrain="3,1,3,3"/>
 <tile id="67" terrain="1,3,3,3"/>
</tileset>
