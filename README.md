# NCMDump.NET

Decipher .ncm file to mp3 || flac

## Feature

Highly Optimized Corelib.

Keep ID3 tags and cover image.

<img src="./README/Result.png"/>

### [Change Log](./Changelog.MD)

## System Requirement

 ```Windows 11 10.0.22000.0``` & ```.NET8 Desktop Runtime``` (Recommended)

 ```Windows 10 10.0.19041.0``` & ```.NET8 Desktop Runtime``` (Minimum)

Linux with dotnet 9 runtime

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
