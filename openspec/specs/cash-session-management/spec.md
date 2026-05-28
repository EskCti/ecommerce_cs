## ADDED Requirements

### Requirement: Open cash session

Operators SHALL open a cash session with initial float after manager PIN verification per RF-050 and RN-040.

#### Scenario: Successful session open

- **WHEN** operator with open terminal submits initial float and valid manager PIN
- **THEN** CashSession is created with Open status linked to operator and terminal

#### Scenario: Open without manager PIN rejected

- **WHEN** operator attempts open without successful manager PIN verification
- **THEN** response status is 403

#### Scenario: Second open session blocked

- **WHEN** operator already has open session for terminal
- **THEN** open request is rejected

### Requirement: Cash withdrawal

Operators or managers SHALL register cash withdrawals (sangria) during open session per RF-055.

#### Scenario: Register withdrawal

- **WHEN** operator registers withdrawal amount during open session
- **THEN** CashWithdrawal is recorded and session withdrawal total increases

### Requirement: Close cash session with breakage

Managers SHALL close cash session with counted cash and calculated breakage per RF-056 and RN-040.

#### Scenario: Close session calculates breakage

- **WHEN** manager closes session with counted cash amount
- **THEN** system calculates breakage as counted minus (opening float plus sales minus withdrawals)

#### Scenario: Close requires manager authorization

- **WHEN** close is attempted without manager PIN verification
- **THEN** request is rejected

### Requirement: Sell only with open session

The system SHALL reject cart and finalize operations when operator has no open cash session.

#### Scenario: Add item without session

- **WHEN** operator adds item without open cash session
- **THEN** response status is 409 indicating session required
