# Log manifest

The simulated logs used by the study. They are **not** copied into the repository: they live in SVELTA's simulated-log bank and are identified here by name, size and SHA-256, so a run can prove which files it used.

Location: `<local folder>\LogsSimulados\<scenario>\runners\<scenario>_<N>casos.rnr.zip`. Each zip contains `Resources/log.jsonl` and `MetaInfo/__ModelInfo`. The files are simulated (no patient data).

| Scenario | Cases | Zip size (MB) | SHA-256 |
|---|---|---|---|
| `gestion_citas` | 2000 | 0.3 | `46438a0c4956b307696b41bbcf585ce020ba1e6a288467afa49ed6795b72ec5d` |
| `gestion_citas` | 10000 | 1.4 | `c0113c158cd32be02a0c70588e9f904cd1babffca63d52cdff99be0acfac681d` |
| `gestion_citas` | 50000 | 6.7 | `e54e8d234071a00d3e177113d62416f05520e948bf5e044c240a8995ba29dd9c` |
| `gestion_citas` | 200000 | 26.9 | `8735ee90de6ce2b2473bd676d3b0b9d17ab3342bf6b8429903334e256ae7df74` |
| `sepsis_hospital_complejo` | 2000 | 1.1 | `1620d987744b84faf0652f6d5d9705584a5904f846c870d8b1d5ae3d6ac3ecc9` |
| `sepsis_hospital_complejo` | 10000 | 5.2 | `988512562490635a776b6ebdb1c0c7185d0c01e02392b21f3b27e7a99cdc9b2e` |
| `sepsis_hospital_complejo` | 50000 | 25.9 | `4cd655bceba9d3bd192737991a8e21b8e5606ecc5247f9626abbbbc8eef98a5b` |
| `sepsis_hospital_complejo` | 200000 | 103.6 | `9330a02f10e83f1b77d090488f81c6a0dfb601270905906ca15c88296b6cfc99` |
| `ictus` | 2000 | 0.6 | `613873a626136a80e9f161bfb98be3a495c1ff208c73889ae8ea46293e583931` |
| `ictus` | 10000 | 3.0 | `1e31bc25bdd836c4f276daf377fbf707d14ffa60b1d1df918bbccaa4f9768750` |
| `ictus` | 50000 | 15.1 | `119f776c42ee07ad0a1ee3b80b6da628f918abf1e835f73a740a3528fa9b5e7b` |
| `ictus` | 200000 | 60.2 | `33b496b88b6e9df61061d1a4d784d2df4a35dbf7ced1f5b86872783ae9759af5` |
| `atencion_primaria` | 2000 | 1.5 | `79d475a61d9144f834b835cf00daf2b55d985569ed1e73221280cc7bcf99f906` |
| `atencion_primaria` | 10000 | 7.2 | `b7acd4bc5360a9c2471878909c49bd812df98e6f359cab58ce7556e9b4b3a223` |
| `atencion_primaria` | 50000 | 36.1 | `1f34bfcf1242258cd41b00c99317e839408f404773da9c481cbc87032fee6b2f` |
| `atencion_primaria` | 200000 | 144.0 | `ebde2a787526cef2c541a7ccdf05cc63a2f9858c5300f936c9ca74a69ea5b4a1` |

## Public logs (4TU.ResearchData)

> **Pending:** this manifest does not list the DOI and authors of each public dataset yet. They must be added before publication; each dataset must be cited with them. Data portal: https://data.4tu.nl

Downloaded on 2026-11-07 to `<local folder>\LogsPublicos\<name>\`. Each file's MD5 was checked against the one 4TU publishes. Classic benchmark logs: cite each dataset (DOI and authors) and do not redistribute the files (the 4TU metadata says *General Terms of Use*; no file states a Creative Commons licence).

### Downloaded files

| Folder | File | Size (MB) | SHA-256 |
|---|---|---|---|
| `BPI_Challenge_2012` | `BPI_Challenge_2012.xes.gz` | 3.34 | `5cd9cc16b9bcb20bd4aae45666a5d87479ddbf47e6371618b6ad217174cecdf3` |
| `BPI_Challenge_2012` | `DATA.xml` | 0.01 | `1f539a257941744840984bbe0afa195878020db1305bcd5189bbbfc21305eb26` |
| `BPI_Challenge_2017` | `BPI Challenge 2017.xes.gz` | 29.66 | `183c5e5189282779c811c78c33ff936351b3dd201165d612211fc220936f8249` |
| `BPI_Challenge_2017` | `DATA.xml` | 0.01 | `f56bbabf3f16aa1b12f6e6a0bc423e41d2bf7f8f4222581de64996aeddd68c2e` |
| `BPI_Challenge_2019` | `BPI_Challenge_2019.xes` | 728.56 | `af63bc687fc4152f2123b05c3af7772b37ef3fce2d3f67f812666c9e356baae7` |
| `BPI_Challenge_2020_Domestic_Declarations` | `DomesticDeclarations.xes.gz` | 0.93 | `0bc3312acf0b9ee5ae6f4ebe9df204005ef11665d62f2d21dd04995161d7532b` |
| `BPI_Challenge_2020_Domestic_Declarations` | `README.txt` | 0.00 | `1cd883bbb879375ffdf34c2616a7184e01dc57d55d4dac2e0d48a230c14eb31f` |
| `BPI_Challenge_2020_International_Declarations` | `InternationalDeclarations.xes.gz` | 1.58 | `b527c8d06255f4312c6ce8f36f7f4110243d84fd6ca48583cd9b80896c601494` |
| `BPI_Challenge_2020_International_Declarations` | `README.txt` | 0.00 | `1cd883bbb879375ffdf34c2616a7184e01dc57d55d4dac2e0d48a230c14eb31f` |
| `BPI_Challenge_2020_Prepaid_Travel_Costs` | `PrepaidTravelCost.xes.gz` | 0.37 | `d2b0f458fe33b808d7134fd464c051364280e2a56c1ee2136419947f103f26f7` |
| `BPI_Challenge_2020_Prepaid_Travel_Costs` | `README.txt` | 0.00 | `1cd883bbb879375ffdf34c2616a7184e01dc57d55d4dac2e0d48a230c14eb31f` |
| `BPI_Challenge_2020_Request_For_Payment` | `README.txt` | 0.00 | `1cd883bbb879375ffdf34c2616a7184e01dc57d55d4dac2e0d48a230c14eb31f` |
| `BPI_Challenge_2020_Request_For_Payment` | `RequestForPayment.xes.gz` | 0.70 | `028e2d645153190200b2ea998747efa02cf1beb6f83db62598963cf391f114b1` |
| `BPI_Challenge_2020_Travel_Permit_Data` | `PermitLog.xes.gz` | 1.94 | `9e23befe5394c13caff4428c00a6cee08647b1749f270a78cd05d99f861c5a8b` |
| `BPI_Challenge_2020_Travel_Permit_Data` | `README.txt` | 0.00 | `1cd883bbb879375ffdf34c2616a7184e01dc57d55d4dac2e0d48a230c14eb31f` |
| `Helpdesk` | `finale.csv` | 3.27 | `31024fa6da0a35578643d50f2bea6e90d5ce97628b64851e271b521f005eef1c` |
| `Helpdesk` | `readme.txt` | 0.00 | `44364ad48c19389687b012f9fa9dd471e176ead0a457a2b2812906fd68a5013a` |
| `Hospital_Billing` | `DATA.xml` | 0.01 | `5cf6ef9ce77f82128e4eb461ccc6d0448336b25b07020fdfe33135aab0f20d10` |
| `Hospital_Billing` | `Hospital Billing - Event Log.xes.gz` | 6.62 | `3f4bb8a09b1d3dec7deb8a8b1c2f31374fbe57fd60fe38477f1cc06989242ef6` |
| `Hospital_Billing` | `readme.txt` | 0.00 | `7693bf521dc6568a808e486cdd6d085d1cbfe1155e33186f7806b4c9f4acec44` |
| `Road_Traffic_Fine` | `DATA.xml` | 0.01 | `e5eff40c95f3ea59eda8c255c7d8f86000b45f9caa001161f43520ffb2fbc652` |
| `Road_Traffic_Fine` | `Road_Traffic_Fine_Management_Process.xes.gz` | 3.45 | `dc9e0e65c964c8ce2a720687ecf17d8204a5cced5db2267c717ed39ada665da8` |
| `Sepsis_Cases` | `DATA.xml` | 0.01 | `12ae0401d150342d86cac078daa87c415694e4ba812f0f699dc27a087525cad3` |
| `Sepsis_Cases` | `Sepsis Cases - Event Log.xes.gz` | 0.20 | `709c52340306415952811b9b9c5dc6bcc8f8d47d583eba39df9a538459dc543a` |
| `Sepsis_Cases` | `readme.txt` | 0.00 | `4a7d7c259669a78989c5d8420a5805f3e2cb17910e4bb89aecde701a6d4458b6` |

### Runner zips generated from them

Generated files; a different conversion would give different hashes. The conversion rules and the per-variable decisions are inside each zip, in `Resources/conversion.md`.

| Folder | Runner zip | Size (MB) | SHA-256 |
|---|---|---|---|
| `BPI_Challenge_2012` | `bpic2012.rnr.zip` | 1.69 | `2980c4f53190f6aefabb141135f0f66a4ea7a075666cf9d95782ac5fd2d005f7` |
| `BPI_Challenge_2017` | `bpic2017.rnr.zip` | 4.81 | `f033a4e391c5657c6ef16fc2db956d8e072c8029e8de922fa0dd3e2930bbeac8` |
| `BPI_Challenge_2019` | `bpic2019.rnr.zip` | 8.98 | `9b32dd54229560e02090fccdff6cb0b9d39037e9f71dac3ac4e7f0bf71f5365d` |
| `BPI_Challenge_2020_Domestic_Declarations` | `bpic2020_domestic_declarations.rnr.zip` | 0.64 | `d42e332a9a9910316c99dcf3a3ca26c8072c3bf299ecd0befd8f819c860b36ad` |
| `BPI_Challenge_2020_International_Declarations` | `bpic2020_international_declarations.rnr.zip` | 1.01 | `11dc038eef7f9cb5e9a4d35a266ee610f6baf257bc7914840d1f213701fb240c` |
| `BPI_Challenge_2020_Prepaid_Travel_Costs` | `bpic2020_prepaid_travel_costs.rnr.zip` | 0.24 | `6837f35ca47726bc73bc6967fca8ac491607c2a8e4301b5d5929eb38872e85af` |
| `BPI_Challenge_2020_Request_For_Payment` | `bpic2020_request_for_payment.rnr.zip` | 0.44 | `9be7d546071618930a5c4e93d7441c36fcc7f001350c3589278c738a9c18f53d` |
| `BPI_Challenge_2020_Travel_Permit_Data` | `bpic2020_travel_permit.rnr.zip` | 1.25 | `d43c354d4f16787f72c36219718fbb6c575d1acbe8eb1295a450798b88e0bbd2` |
| `Helpdesk` | `helpdesk.rnr.zip` | 0.27 | `d2700d60b165421934f323969b4bca90f250ac195d84b93f6fbcdbdf466d58e9` |
| `Hospital_Billing` | `hospital_billing.rnr.zip` | 5.29 | `073d42955b91e0658d7bec2b592df3e1fa97aa2f58f1e375a61382b35cce5b0f` |
| `Road_Traffic_Fine` | `road_traffic_fine.rnr.zip` | 2.72 | `8afaab668020c89f9e4f156d7af59462d6e66fcd3d7d1cbb195a4b8e1709ad23` |
| `Sepsis_Cases` | `sepsis_cases.rnr.zip` | 0.17 | `0eff919de240cbd493755944b2e3f07d5c43e6b33330813434d875f9fa91d79a` |
