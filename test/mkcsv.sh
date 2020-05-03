#!/bin/sh

N0=1
N=10000
Year=2020
for Month in 4
do

  Day=1 
  ruby > test-$Year-$Month.csv <<EOM
  require "date"

  LABEL = "ID,名前,日付,体温,せき,息苦しさ,鼻水,のどの痛み,体のだるさ,下痢,頭痛,その他風邪症状,その他症状詳細,解熱剤・せき止め薬・かぜ薬等の服用,検査実施,検査結果"

  date = Date.new($Year, $Month, 1)
  PREFIX="KN"
  puts LABEL

  while date.month == $Month
    for i in ($N0..$N)
      id = sprintf("#{PREFIX}%d", i)
      name = sprintf("dummyuser%05d", i)
      measured_date = sprintf("%04d/%02d/%02d", date.year, date.month, date.day)
      d = sprintf("%4.1f", 36 + rand()*2)
      cs = (1..8).map{|c| rand() < 0.99 ? "無" : "有" }.join(",")
      puts "#{id},#{name},#{measured_date},#{d},#{cs},,N,N,"
    end
    date = date + 1
  end
EOM
  nkf -s test-$Year-$Month.csv > $Year-$Month.csv
  rm test*
done

