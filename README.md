# NCMDump.NET

Decipher .ncm file to mp3 || flac

## Feature

Highly Optimized Corelib.

Keep ID3 tags and cover image.

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/README/Result.png"/>

## System Requirement

 ```Windows 11 10.0.22000.0``` & ```.NET8 Desktop Runtime``` (Recommended)

 ```Windows 10 10.0.19041.0``` & ```.NET8 Desktop Runtime``` (Minimum)

## Changelog

### v2.2.0 2024.03.24

Code Cleaning.

### v2.1.0 2023.11.19

Fix some UI problem on Windows 10.

Windows 10 might not be fully supported in future.

### v2.0.0 2023.11.17

Move to .NET 8.

[Runtime Download Link](https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-desktop-8.0.0-windows-x64-installer ".NET 8.0 Desktop Runtime (v8.0.0) - Windows x64 Installer")

Add Support for cloud music 3.0.0(beta)

UI Remake.

Dark mode available now. 

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/README/light.png"/>

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/README/dark.png"/>

### v1.6.2 2023.08.05

Critical Bug Fix

UI Improvment

Async Optimized

SIMD Optimized

code cleaning

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/README/Demo1.6.png"/>


### v1.5

Upgrade to .Net 7 Runtime with so Fxxxxxx high performance.
[Runtime Download Link](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.4-windows-x64-installer ".NET 7.0 Desktop Runtime (v7.0.4) - Windows x64 Installer")

Add check box: Delete .ncm file when done 

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/README/Demo1.5.png"/>

### v1.4

UI adjustment.

Performance Improvement.

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/README/Demo1.4.png"/>

### v1.3

Bug fix.

Async optimization.

### v1.2

Bug fix.

Drag directory || Select folder on GUI support.

### v1.1

Drag directory on executable support.

Bug fix

## Usage

### CLI

Drag .ncm file or directory on exe

### GUI

Don't ask, Just use.

## API

### public bool Convert(string path)

Convert NCM file into MP3/FLAC format in synchronous.

#### Parameters

```path``` String

File Path to a NCM file.

#### Return

```bool``` True if convert success

<br/>

### public async Task\<bool\> ConvertAsync(string path)

Convert NCM file into MP3/FLAC format in asynchronous.

#### Parameters

```path``` String

File Path to a NCM file.

#### Return

```bool``` True if convert success

## Refrence
<https://github.com/mono/taglib-sharp>

<https://github.com/lepoco/wpfui>

<https://github.com/anonymous5l/ncmdump>

## Stargazers over time

[![Stargazers over time](https://starchart.cc/kingsznhone/NCMDump.NET.svg)](https://starchart.cc/kingsznhone/NCMDump.NET)
