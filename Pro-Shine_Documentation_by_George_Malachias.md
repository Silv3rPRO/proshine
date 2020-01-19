Software Requirements Specification

for

Pro-Shine

![](media/841e9cfdf4588ca41df6bcda98bc7c60.png)

Version 1.0

Prepared by Gewrgios Malachias

14/1/2020

Content Table
=============

[1. Introduction 1](#introduction)

[1.1 Purpose 1](#purpose)

[1.2 Document Conventions 1](#document-conventions)

[1.3 Intended Audience and Reading Suggestions
1](#intended-audience-and-reading-suggestions)

[1.4 Project Scope 1](#project-scope)

[1.5 References 1](#references)

[2. Overall Description 2](#overall-description)

[2.1 Product Functions 2](#product-functions)

[2.2 User Classes and Characteristics 2](#user-classes-and-characteristics)

[2.3 Operating Environment 2](#operating-environment)

[2.4 Design and Implementation Constraints
2](#design-and-implementation-constraints)

[3. External Interface Requirements 3](#external-interface-requirements)

[3.1 User Interfaces 3](#user-interfaces)

[4. System Features 4](#system-features)

[4.1 Automation of most actions needed to play the game
4](#automation-of-most-actions-needed-to-play-the-game)

[4.2 Evade Staff Members 4](#evade-staff-members)

[4.3 Auto – Reconnect 4](#auto-reconnect)

[4.4 Add Graphical User Interface (GUI) 4](#auto-reconnect)

Revision History

| **Name**               | **Date** | **Reason For Changes**                            | **Version** |
|------------------------|----------|---------------------------------------------------|-------------|
| **Gewrgios Malachias** | 10/01/19 | Initial Software Requirements Specification draft | 1.0         |
|                        |          |                                                   |             |

Introduction
============

Purpose
-------

This Document intends to describe and analyze, the potential and the
requirements of the PRO – Shine open source software.

Document Conventions
--------------------

Everything underlined is a website. Use control + click to move to the website.

Intended Audience and Reading Suggestions
-----------------------------------------

The Document is intended for every programmer/developer that wants to learn more
about the open source project and the client/user that needs more information
about the project.

Project Scope
-------------

**Pro - Shine** is a free, open source and advanced bot for Pokemon Revolution
Online. It’s main purpose is to automate some activities in Pokemon Revolution
Online. Pro – Shine supports only Windows for the moment, but Linux and MacOS
might be supported in the future, using Mono or .Net Core.

References
----------

<https://proshine-bot.com/>

<https://github.com/DubsCheckum/proshine>

Overall Description
===================

Product Functions
-----------------

Write down your own scripts, or even use some that someone else has written and
automate every action in Pokemon Revolution Online. All you need to do is to
know how to write Lua scripts.

User Classes and Characteristics
--------------------------------

Most of the people use this program, because Pokemon Revolution Online is pretty
time consuming, and requires to spend many hours in order to finish it.

Operating Environment
---------------------

The program is available only in Windows  
Here are the supported versions:

-   Windows Vista ( Service Pack 2)

-   Windows 7 ( Service Pack 1 )

-   Windows 8.1

-   Windows 10

Design and Implementation Constraints
-------------------------------------

Pro – Shine consumes about 30MB memory. It’s written in C\# language. It
requires following library to build and run:

.NET Framework 4.5.2.

External Interface Requirements
===============================

User Interfaces
---------------

Windows GUI:

![](media/1182c6a1a612521344e4f28f166d8dad.png)

Windows GUI gives the user many options that we list below:

-   Auto - evolve Pokemon

-   Auto – reconnect

-   Viewing and adjusting your team

-   Looking in your inventory and giving items

-   Spectating the chat

-   Spectating the players nearby

-   An image of the map

-   Which Pokemon you are against.

System Features
===============

Automation of most actions needed to play the game
--------------------------------------------------

Pro - Shine gives you the potential to write scipts that will allow you to do
everything inside the game automatically.This means that you can catch pokemon,
level up your team, fight npc trainers, fish to catch pokemon and whatever else
you are able to script.

Evade Staff Members
-------------------

You can turn-on the option to evade the staff members and by doing so, whenever
a Game Master comes nearby your

>   account will be logged out of the game.

Auto – Reconnect
----------------

Also there is the option to auto – reconnect in that case whenever you get
kicked out by the game, you automatically reconnect into it.

Add Graphical User Interface (GUI)
----------------------------------

>   It is also possible for someone to add GUI to the original program just by
>   adding some lines inside the lua script someone is using.

>   Original GUI:

![](media/c58b4c794c5411de36e936805aaca905.png)

>   GUI with lua Script:

![](media/25cd022b76e6db9a910fd1b231791276.png)
