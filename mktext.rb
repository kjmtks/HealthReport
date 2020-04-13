LABEL = "ID,名前,日付,体温,せき,息苦しさ,鼻水,のどの痛み,体のだるさ,下痢,頭痛,その他風邪症状,その他症状詳細,解熱剤・せき止め薬・かぜ薬等の服用,検査実施,検査結果"

N=20
PREFIX="KN3"

puts LABEL
for k, _j in [[1,31],[2,29],[3,31]]
  for j in (1.._j)
    for i in (1..N)
      id = sprintf("#{PREFIX}%02d", i)
      name = "dummy"
      date = sprintf("%04d/%02d/%02d", 2020, k, j)
      d = sprintf("%4.1f", 36 + rand()*2)
      cs = (1..8).map{|c| rand() < 0.99 ? "無" : "有" }.join(",")
      puts "#{id},#{name},#{date},#{d},#{cs},,N,N,"
    end
  end
end
